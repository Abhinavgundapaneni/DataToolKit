using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataToolKit.Models;
using DataToolKit.DataAccess;
using X.PagedList;
using DataToolKit.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;


namespace DataToolKit.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly IConfiguration _configuration;

        private readonly IWebHostEnvironment _environment;

        private readonly SignInManager<DataToolKitUser> _signInManager;

        private readonly Helper _helper;

        public IPagedList<BatchControl> Batches { get; private set; }

        public IndexModel(SignInManager<DataToolKitUser> signInManager, ILogger<IndexModel> logger, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _logger = logger;
            _signInManager = signInManager;
            _configuration = configuration;
            _environment = environment;
            var dummyData = new List<BatchControl>();
            _helper = new Helper();
            Batches = new PagedList<BatchControl>(dummyData, pageNumber: 1, pageSize: 3);

        }

        [BindProperty]
        public BatchControl Batch { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public int CurrentPage { get; private set; }

        public int PageSize { get; private set; }

        public class InputModel
        {
            [EmailAddress]
            public string? Email { get; set; }

            [DataType(DataType.Password)]
            public string? Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public IActionResult OnGetLoadBatchData(int batchId)
        {
            try {
                // Load batch data by batchId (retrieve uploaded data)
                DTKDB db = new DTKDB(_configuration);
                List<BatchDataFile> batchDataFile = db.GetBatchData(batchId);
                
                return Partial("_BatchDataPartial", batchDataFile); // Create a partial view for displaying batch data
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                // Return an error response
                return StatusCode(500, new { error = ex.Message });
            }

        }

        public IActionResult OnGet(int? pagenumber, string? sortOrder, string? searchColumn, string? searchString, int? pageSize)
        {
            TempData["UploadSuccessMessage"] = "";
            PageSize = pageSize ?? 10;

            // Load batch data from your data source and populate Batches
            LoadBatchData(pagenumber, sortOrder, searchColumn, searchString);
           
            return Page();
        }

        public IActionResult OnPost()
        {
            int BatchId = 0;
            string fileName = "";

            if (ModelState.IsValid)
            {
                DTKDB db = new DTKDB(_configuration);

                IFormFile uploadedFile = Request.Form.Files["postedFile"];

                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    fileName = uploadedFile.FileName;
                    Batch.InputFileName = fileName;
                    Batch.SubmitName = User.Identity?.Name;
                }

                // Save batch data and upload file here
                BatchId = db.InsertBatchControl(Batch);
                fileName = BatchId.ToString() + "_" + fileName;

                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    //string path = _configuration["UploadedFilePath"];
                    string path = Path.Combine(this._environment.WebRootPath, "UploadFiles");                   

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    //Save the uploaded Excel file.
                    string filePath = Path.Combine(path, fileName);

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadedFile.CopyTo(stream);
                    }

                    string csvData = System.IO.File.ReadAllText(filePath);

                    bool firstRow = true;

                    foreach (string row in csvData.Split('\n'))
                    {
                        if (!string.IsNullOrEmpty(row))
                        {
                            if (!string.IsNullOrEmpty(row))
                            {
                                if (firstRow)
                                    firstRow = false;
                                else
                                {
                                    BatchDataFile BDF = new BatchDataFile();
                                    var values = SplitCsv(row);

                                    BDF.BatchId = BatchId;
                                    BDF.NPI = Convert.ToInt32(values[0]);
                                    BDF.Segment = values[1];
                                    string str = db.InsertBatchDataFile(BDF);

                                    // Upload successful, set a success message
                                    TempData["UploadSuccessMessage"] = "Batch upload was successful.";
                                } 
                            }
                        }
                    }
                    //if(System.IO.File.Exists(filePath))
                    //{
                    //    System.IO.File.Delete(filePath);
                    //}
                    

                }
                    

                string rtnMessage = db.UpdateBatchControlFile(BatchId);

            }
            

            // Reload batch data after saving
            LoadBatchData(1, "", null, null);
            OnPostClearForm();

            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect("/Home/Index");
                }
                
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
        private void LoadBatchData(int? pagenumber, string? sortOrder, string? searchColumn, string? searchString)
        {
            // Define sorting options
            ViewData["CurrentSort"] = sortOrder;
            ViewData["BatchIdSortParam"] = string.IsNullOrEmpty(sortOrder) ? "batchid_desc" : "";
            ViewData["VendorNameSortParam"] = sortOrder == "vendorname" ? "vendorname_desc" : "vendorname";

            // Load batch data from the service
            var batches = LoadBatchDataFromDatabase();
            if (_helper.isLoggedInUserAdmin(User) == false)
            {
                // If logged in user is not admin, filter out batches.
                batches = batches.Where(x => x.SubmitName == User.Identity?.Name).ToList();
            }

            if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchColumn))
            {
                switch(searchColumn)
                {
                    case "customerName":
                        batches = batches.Where(x => x.CustomerName.ToLower().Contains(searchString.ToLower())).ToList();
                        break;
                    case "ProjectCode":
                        batches = batches.Where(x => x.ProjectCode.ToLower().Contains(searchString.ToLower())).ToList();
                        break;
                    case "reportName":
                        batches = batches.Where(x => x.ReportTitle.ToLower().Contains(searchString.ToLower())).ToList();
                        break;
                    case "requestType":
                        batches = batches.Where(x => x.RequestTypeCode.ToLower().Contains(searchString.ToLower())).ToList();
                        break;
                    case "SubmitDate":
                        batches = batches.Where(x => x.SubmitDate.ToLower().Contains(searchString.ToLower())).ToList();
                        break;
                    case "SubmitName":
                        batches = batches.Where(x => x.SubmitName.ToLower().Contains(searchString.ToLower())).ToList();
                        break;
                    case "StatusDescription":
                        batches = batches.Where(x => x.StatusDescription.ToLower().Contains(searchString.ToLower())).ToList();
                        break;
                    default:
                        break;
                } 
                
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "batchid_desc":
                    batches = batches.OrderBy(b => b.BatchId).ToList();
                    break;
                case "vendorname":
                    batches = batches.OrderBy(b => b.VendorName).ToList();
                    break;
                case "vendorname_desc":
                    batches = batches.OrderByDescending(b => b.VendorName).ToList();
                    break;
                default:
                    batches = batches.OrderByDescending(b => b.BatchId).ToList();
                    break;
            }

            CurrentPage = pagenumber ?? 1;
            PageSize = PageSize > 0 ? PageSize : 10;

            // Create a StaticPagedList from the sorted list

            Batches = batches.ToPagedList(CurrentPage, PageSize);
        }

        private List<BatchControl> LoadBatchDataFromDatabase()
        {
            DTKDB db = new DTKDB(_configuration);
            return db.GetBatchControls();
        }

        private List<string> SplitCsv(string csv)
        {
            var values = new List<string>();

            int last = -1;
            bool inQuotes = false;

            int n = 0;
            while (n < csv.Length)
            {
                switch (csv[n])
                {
                    case '"':
                        inQuotes = !inQuotes;
                        break;
                    case ',':
                        if (!inQuotes)
                        {
                            values.Add(csv.Substring(last + 1, (n - last)).Trim(' ', ','));
                            last = n;
                        }
                        break;
                }
                n++;
            }

            if (last != csv.Length - 1)
                values.Add(csv.Substring(last + 1).Trim());

            return values;
        }

        public void OnPostClearForm()
        {
            // Clear the form fields by resetting the values in your PageModel
            Batch.VendorName = string.Empty; 
            Batch.CustomerName = string.Empty;
            Batch.InputFileName = string.Empty;
            Batch.ResultEmail1 = string.Empty;
            Batch.DescriptionTitle = string.Empty; 
            Batch.ProjectCode = string.Empty;
            Batch.ReportTitle = string.Empty;
            Batch.RequestTypeCode = string.Empty;
            Batch.SubmitName = string.Empty;
        }

    }
}

