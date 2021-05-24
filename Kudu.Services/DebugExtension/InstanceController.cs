﻿using Kudu.Core.K8SE;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kudu.Services.DebugExtension
{
    [Route("/instances")]
    public class InstanceController : Controller
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        //private static readonly MemoryCache _cache = new MemoryCache();

        [HttpGet]
        public async Task<List<PodInstance>> GetInstances()
        {
            if(K8SEDeploymentHelper.IsK8SEEnvironment())
            {
                return K8SEDeploymentHelper.GetInstances(K8SEDeploymentHelper.GetAppName(HttpContext));
            }

            return null;
        }

        [HttpGet]
        [Route("{instanceId}")]
        public async Task<PodInstance> GetInstance(string instanceId)
        {
            if (K8SEDeploymentHelper.IsK8SEEnvironment())
            {
                var instances = K8SEDeploymentHelper.GetInstances(K8SEDeploymentHelper.GetAppName(HttpContext));
                PodInstance instance = null;
                if (instances.Count > 0)
                {
                    instance = instances.Where(i => i.Name.Equals(instanceId, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                }

                if (instances.Count > 0 && instanceId.Equals("any", System.StringComparison.OrdinalIgnoreCase))
                {
                    instance = instances[0];
                }

                return instance;
            }

            return null;
        }

        /*
        [Route("{instanceId}/gcdump")]

        public async Task<HttpResponseMessage> GetGcDump(string instanceId)
        {
            var instances = K8SEDeploymentHelper.GetInstances(K8SEDeploymentHelper.GetAppName(HttpContext));
            PodInstance instance = null;
            if (instances.Count > 0)
            {
                instance = instances.Where(i => i.Name.Equals(instanceId, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }

            if (instances.Count > 0 && instanceId.Equals("any", System.StringComparison.OrdinalIgnoreCase))
            {
                instance = instances[0];
            }

            if (instance == null)
            {
                return NotFound("Invalid instance");
            }

            var httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            using (var httpClient = new HttpClient())
            {
                string absoluteUrl = _baseUri.ToString() + "/" + url + Request.RequestUri.Query;
                var proxyRequest = new HttpRequestMessage(Request.Method, absoluteUrl);
                foreach (var header in Request.Headers)
                {
                    proxyRequest.Headers.Add(header.Key, header.Value);
                }

                return await httpClient.SendAsync(proxyRequest, HttpCompletionOption.ResponseContentRead);
            }
        }

        
        [Route("{instanceId}/remotedebug")]
        public IActionResult RemoteDebug(string instanceId)
        {

            var instances = K8SEDeploymentHelper.GetInstances(K8SEDeploymentHelper.GetAppName(HttpContext));
            PodInstance instance = null;
            if (instances.Count > 0)
            {
                instance = instances.Where(i => i.Name.Equals(instanceId, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }

            if (instances.Count > 0 && instanceId.Equals("any", System.StringComparison.OrdinalIgnoreCase))
            {
                instance = instances[0];
            }

            if (instance == null)
            {
                return "Invalid instance";
            }

            using (Socket testSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                testSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                testSocket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), debugPort));
                context.Response.StatusCode = 200;
                _logger.LogInformation("GetStats success " + ipAddress + ":" + debugPort);
                if (IsV2StatusAPIRequest)
                {
                    var response = new LSiteStatusResponse(lSiteStatus, debugPort, true);
                    var json = JsonConvert.SerializeObject(response);
                    await context.Response.WriteAsync(json);
                }
                else
                {
                    await context.Response.WriteAsync("SUCCESS:" + debugPort);
                }
            }
        }
        */

        /*
        [Route("{instanceId}/gcdump")]
        public async Task<string> GetGCDump(string instanceId)
        {
            var httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            var webRequest = new HttpClient(httpClientHandler);

            var buffer = new byte[4 * 1024];
            var localResponse = HttpContext.Response;
            try
            {
                using (var remoteStream = await webRequest.GetStreamAsync(url))
                {
                    var bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                    localResponse.Clear();
                    localResponse.ContentType = "application/octet-stream";
                    var uri = "";
                    var scheme = HttpContext.Request.Scheme;
                    if (scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                    {
                        scheme = "http";
                    }
                    if (HttpContext.Request.Path.StartsWithSegments($"/instances/{instanceId}", out var remainingPath))
                    {
                        Console.WriteLine("PATH STRING : " + remainingPath);
                        uri = $"{scheme}://localhost:3000" + remainingPath);
                    }
                    var fileName = Path.GetFileName(url);
                    localResponse.Headers.Add("Content-Disposition", "attachment; filename=" + fileName);

                    if (remoteStream.Length != -1)
                        localResponse.ContentLength = remoteStream.Length;

                    while (bytesRead > 0) // && localResponse.IsClientConnected)
                    {
                        await localResponse.Body.WriteAsync(buffer, 0, bytesRead);
                        bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception e)
            {
                // Do some logging here
            }
        }
        

        [Route("{instanceId}/gcdump")]
        public async Task<string> GetGcDump(string instanceId)
        {
            var instances = K8SEDeploymentHelper.GetInstances(K8SEDeploymentHelper.GetAppName(HttpContext));
            PodInstance instance = null;
            if (instances.Count > 0)
            {
                instance = instances.Where(i => i.Name.Equals(instanceId, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }

            if (instances.Count > 0 && instanceId.Equals("any", System.StringComparison.OrdinalIgnoreCase))
            {
                instance = instances[0];
            }

            if (instance == null)
            {
                return "Invalid instance";
            }

            var httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            var webRequest = new HttpClient(httpClientHandler);
            var url = $"http://{instance.IpAddress}:1601/dump";
            var buffer = new byte[4 * 1024];
            var localResponse = HttpContext.Response;
            try
            {
                using (var remoteStream = await webRequest.GetStreamAsync(url))
                {
                    var bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                    localResponse.Clear();
                    localResponse.ContentType = "application/octet-stream";  
                    var uri = "";
                    var scheme = HttpContext.Request.Scheme;
                    if (scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                    {
                        scheme = "http";
                    }
                    if (HttpContext.Request.Path.StartsWithSegments($"/instances/{instanceId}", out var remainingPath))
                    {
                        Console.WriteLine("PATH STRING : " + remainingPath);
                        uri = $"{scheme}://{instance.IpAddress}:1601{remainingPath}";
                        Console.WriteLine($"PATH STRING : {scheme}://{instance.IpAddress}:1601{remainingPath}");

                    }
                    var fileName = Path.GetFileName(url);
                    localResponse.Headers.Add("Content-Disposition", "attachment; filename=" + fileName);

                    if (remoteStream.Length != -1)
                        localResponse.ContentLength = remoteStream.Length;

                    while (bytesRead > 0) // && localResponse.IsClientConnected)
                    {
                        await localResponse.Body.WriteAsync(buffer, 0, bytesRead);
                        bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                    }

                    return File(bytesRead, "application/vnd.openxmlformats", "fileName.xlsx");
                }
            }
            catch (Exception e)
            {
                // Do some logging here
            }
        }

        /*
        public async Task<string> SSH(string instanceId, string subpath)
        {
            if(K8SEDeploymentHelper.IsK8SEEnvironment())
            {
                
                var instances = K8SEDeploymentHelper.GetInstances(K8SEDeploymentHelper.GetAppName(HttpContext));
                PodInstance instance = null;
                if (instances.Count > 0)
                {
                    instance = instances.Where(i => i.Name.Equals(instanceId, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                }

                if(instances.Count > 0 && instanceId.Equals("any", System.StringComparison.OrdinalIgnoreCase))
                {
                    instance = instances[0];
                }

                if(instance == null)
                {
                    return "Invalid instance";
                }
                
                var instance = new PodInstance()
                {
                    Name = "codeapp-sample-8994dbf4d-vsdr5",
                    IpAddress = "10.244.1.62",
                    NodeName = "node",
                };

                var targetUri = BuildTargetUri(HttpContext.Request, instanceId);
                var targetRequestMessage = CreateTargetMessage(HttpContext, targetUri, instance);

                using (var responseMessage = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted))
                {
                    //HttpContext.Response.StatusCode = (int)responseMessage.StatusCode;
                    //CopyFromTargetResponseHeaders(HttpContext, responseMessage);
                    await responseMessage.Content.CopyToAsync(HttpContext.Response.Body);
                }
                
            }
            return null;
        }

        */
        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri, PodInstance instance)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);
            if(!requestMessage.Headers.Contains("WEBSITE_SSH_USER"))
            {
                requestMessage.Headers.Add("WEBSITE_SSH_USER", "root");
            }
            if (!requestMessage.Headers.Contains("WEBSITE_SSH_PASSWORD"))
            {
                requestMessage.Headers.Add("WEBSITE_SSH_PASSWORD", "Docker!");
            }
            if (!requestMessage.Headers.Contains("WEBSITE_SSH_IP"))
            {
                requestMessage.Headers.Add("WEBSITE_SSH_IP", instance.IpAddress);
            }
            return requestMessage;
        }

        private void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
              !HttpMethods.IsHead(requestMethod) &&
              !HttpMethods.IsDelete(requestMethod) &&
              !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
        }
        private static HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }

        private Uri BuildTargetUri(HttpRequest request, string instanceId)
        {
            Uri targetUri = null;
            var scheme = request.Scheme;
            if(scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                scheme = "http";
            }
            if (request.Path.StartsWithSegments($"/instances/{instanceId}", out var remainingPath))
            {
                Console.WriteLine("PATH STRING : " + remainingPath);
                targetUri = new Uri($"{scheme}://localhost:3000" + remainingPath);
            }

            return targetUri;
        }
    }
}
