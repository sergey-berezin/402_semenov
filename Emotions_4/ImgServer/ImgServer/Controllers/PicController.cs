using Contracts;
using ImgServer;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace WebApplication1.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ImagesController : Controller
    {
        private I_Db dB;

        public ImagesController(I_Db db)
        {
            this.dB = db;
        }

        [HttpGet]
        public async Task<List<int>> GetImages()
        {
            var result = await dB.GetAllImages();
            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DBpic>> GetImages(int id)
        {
            var result = await dB.TryGetImageById(id);
            if (result != null)
                return result;
            return StatusCode(404, "Server error");
        }

        [HttpDelete("{id}")]
        public async Task<int> DeleteImages(int id)
        {
            var result = await dB.TryDeleteImage(id);
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<string>> PostImages([FromBody] string path, CancellationToken token)
        {
            return await dB.PostImage(path, token);
        }

    }
}
