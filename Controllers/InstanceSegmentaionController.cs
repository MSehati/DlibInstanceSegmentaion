using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLog;
using Microsoft.AspNetCore.Http;
using System.IO;
using DlibInstanceSegmentaion.Model;
using DlibInstanceSegmentaion.General;

namespace DlibInstanceSegmentaion.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Produces("application/json")]
    public class InstanceSegmentaionController : ControllerBase
    {
        private static NLog.Logger log = LogManager.GetCurrentClassLogger();

        [HttpPost]
        public async Task<SegmentInfo> GetInstances(IFormFile imageFile)
        {
            try
            {
                string originalFolder = "original";
                if (imageFile.Length > 0)
                {
                    string[] splitArray = imageFile.FileName.Split(".");
                    if (!Directory.Exists(originalFolder))
                        Directory.CreateDirectory(originalFolder);
                    string imageId = Guid.NewGuid().ToString();
                    string filePath = originalFolder + "/" + imageId + "." + splitArray[1];  //Path.Combine(uploads, uploadFile.picture.FileName);
                    using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    return InstanceSegmentaion.DoSegmentaion(filePath, imageId);
                }
                return null;
            }
            catch (Exception e)
            {
                log.Error("error in GetImage method not notify");
                log.Error(e);
                SegmentInfo segmentInfo = new SegmentInfo();
                segmentInfo.error = e.Message;
                return segmentInfo;
            }      
                    
        }

        [HttpGet]
        public async Task<IActionResult> Download(string filename)
        {
            try
            {
                if (filename == null)
                    return Content("filename not present");
                string path = filename;

                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, "image/png", Path.GetFileName(path));
            }
            catch (Exception e)
            {
                log.Error("error in Download method");
                log.Error(e);
                return null;
            }

        }
    }
}
