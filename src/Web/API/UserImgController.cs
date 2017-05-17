namespace Web.API
{
    using Common;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Platform.Util;
    using Service.Media;
    using System.Collections.Generic;
    using Entity.Media;
    using Platform.Model;
    using Microsoft.Net.Http.Headers;
    using System.IO;
    using System;
    using Platform.Enum;
    using Service.User;
    using Microsoft.AspNetCore.Authorization;

    [Authorize]
    [Route("api/[controller]")]
    public class UserImgController : BaseController
    {
        // GET: api/values
        [HttpGet("{id}")]
        public JsonResult Get(string id)
        {
            try
            {
                var result = new List<ImgInfo>();

                var user = this.GetService<UserService>().Get(id);

                if (user == null || user.Pics == null)
                {
                    return Json(new { status = Result.Success });
                }

                var imgs = this.GetService<ImgService>().GetByIds(user.Pics);

                foreach (var img in imgs)
                {
                    var imgId = img.Id;
                    result.Add(new ImgInfo("../../api/img/" + imgId, "../../api/img/" + imgId + "?height=45&width=80",
                        img.Name, img.ContentType, img.Length, string.Format("../../api/userimg/{0}?userId={1}", imgId, id), "DELETE"));
                }

                return Json(new { files = result });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = Result.Failure,
                    msg = ex.Message
                });
            }
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public JsonResult Put(string id, IFormFile file)
        {
            try
            {
                var img = new Img();
                var result = new List<ImgInfo>();

                if (file.Length > 0)
                {
                    img.Length = (int)file.Length;
                    //get the file's name
                    var parsedContentDisposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);
                    img.Name = parsedContentDisposition.FileName.Trim('"');
                    img.ContentType = file.ContentType;

                    //get the bytes from the content stream of the file
                    byte[] bytes = new byte[file.Length];
                    using (BinaryReader theReader = new BinaryReader(file.OpenReadStream()))
                    {
                        bytes = theReader.ReadBytes((int)file.Length);
                    }

                    //convert the bytes of image data to a string using the Base64 encoding
                    img.Content = bytes;

                    var imgId = this.GetService<ImgService>().Create(img);

                    var userService = this.GetService<UserService>();
                    var user = userService.Get(id);

                    var pics = user.Pics;

                    if(pics == null)
                    {
                        pics = new List<string>();
                    }

                    pics.Add(imgId);

                    userService.Update(id, "Pics", pics);

                    result.Add(new ImgInfo("../../api/img/" + imgId, "../../api/img/" + imgId + "?height=45&width=80",
                        img.Name, img.ContentType, img.Length, string.Format("../../api/userimg/{0}?userId={1}", imgId, id), "DELETE"));
                }

                return Json(new { files = result });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = Result.Failure,
                    msg = ex.Message
                });
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(string id, string userId)
        {
            this.GetService<ImgService>().Delete(id);

            var userService = this.GetService<UserService>();
            var user = userService.Get(userId);

            var pics = user.Pics;
            pics.Remove(id);

            userService.Update(userId, "Pics", pics);
        }
    }
}
