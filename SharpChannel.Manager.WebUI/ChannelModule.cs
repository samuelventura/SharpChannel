using System;
using Nancy;
using Nancy.Json;
using Nancy.Responses;
using Nancy.ModelBinding;
using SharpChannel.Tools;

namespace SharpChannel.Manager.WebUI
{
    public class ChannelModule : NancyModule
    {
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        private bool IsLoopback(string ip)
        {
            //Profile.Trace("IsLoopback {0}", ip);
            //both sent by chrome while debuging 
            if (ip == "127.0.0.1") return true;
            if (ip == "::1") return true;
            return false;
        }

        public ChannelModule(CachedManager manager)
        {
            Get["/"] = _ => {
                //Profile.Trace("/ from:{0}", Request.UserHostAddress);
                return View["ChannelIndex.html", manager.List()];
            };
            //curl http://127.0.0.1:2018/Test
            Get["/Test"] = _ => {
                var data = new
                {
                    this.Request.UserHostAddress,
                    this.Request.Headers,
                    this.Request.Query,
                    this.Request.Form,
                    this.Request.Method,
                    this.Request.Url,
                    this.Request.Path
                };
                return Response.AsText(serializer.Serialize(data), "application/json");
            };
            //curl http://127.0.0.1:2018/Api/Types
            Get["/Api/Types"] = _ => {
                return manager.Plugins;
            };
            //curl http://127.0.0.1:2018/Api/Index
            Get["/Api/Index"] = _ => {
                return manager.List();
            };
            //curl http://127.0.0.1:2018/Api/ById/1
            Get["/Api/ById/{id:int}"] = p => {
                return manager.LoadById(p["id"]);
            };
            //curl http://127.0.0.1:2018/Api/ByName/ECHO1
            Get["/Api/ByName/{name}"] = p => {
                return manager.LoadByName(p["name"]);
            };
            //curl http://127.0.0.1:2018/Api/List/Echo
            //curl http://127.0.0.1:2018/Api/List/Serial
            //curl http://127.0.0.1:2018/Api/List/Modbus
            //curl http://127.0.0.1:2018/Api/List/Socket
            Get["/Api/List/{type}"] = p => {
                return ChannelUtils.List(p["type"]);
            };
            Get["/New{type}Channel"] = p => {
                var instance = new ChannelInstance(p["type"]);
                var view = string.Format("Edit{0}Channel.html", p["type"]);
                return View[view, new ChannelModel(instance)];
            };
            Get["/EditChannel/{id:int}"] = p => {
                var model = manager.LoadById(p["id"]);
                if (model == null) return new RedirectResponse("/");
                var view = string.Format("Edit{0}Channel.html", model.Type);
                return View[view, model];
            };
            Post["/UpdateChannel/{id:int}/{access}"] = p => {
                if (IsLoopback(Request.UserHostAddress))
                {
                    var access = Enum.Parse(typeof(ChannelAccess), p["access"]);
                    manager.Update(p["id"], access);
                }
                return new RedirectResponse("/");
            };
            Post["/SaveChannel"] = p => {
                if (IsLoopback(Request.UserHostAddress))
                {
                    var instance = this.Bind<ChannelInstance>();
                    int id = manager.Save(instance);
                }
                return new RedirectResponse("/");
            };
            Post["/RemoveChannel/{id:int}"] = p => {
                if (IsLoopback(Request.UserHostAddress))
                {
                    manager.Delete(p["id"]);
                }
                return new RedirectResponse("/");
            };
        }
    }
}
