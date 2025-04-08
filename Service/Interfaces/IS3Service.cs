using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Service.Interfaces;

public interface IS3Service
{
    Task<string> UploadFileAsync(IFormFile file, string folderName);
    Task DeleteFileAsync(string fileUrl);
}
