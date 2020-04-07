using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cw3.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            if (context.Request != null)
            {
                string path = context.Request.Path;
                string method = context.Request.Method;
                string queryString = context.Request.QueryString.ToString();
                string bodyStr = "";

                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                }

                string log = DateTime.Now + " " + path + " " + method + " " + queryString + bodyStr;
                string logPathFile = "logs.txt";
                if (!File.Exists(logPathFile))
                {
                    using (StreamWriter sw = File.CreateText(logPathFile))
                    {
                        sw.WriteLine(log);
                    }
                }
                using (StreamWriter sw = File.AppendText(logPathFile))
                {
                    sw.WriteLine(log);
                }
            }



            context.Request.Body.Seek(0, SeekOrigin.Begin);
            await _next(context);
        }
    }
}
