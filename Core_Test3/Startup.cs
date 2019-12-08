using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UploadHandle;

namespace Core_Test3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();


            //�����ϴ�
            UploadHandle.ServerInfo.SitePath = env.WebRootPath; //ʹ��wwwroot��Ϊ��Ŀ¼

            //��WebScoket���� ,ָ��websocket ��ʽ���������
            app.Map("/common/socket", (con) =>
            {
                con.UseWebSockets();
                UploadHandle.Receiver.Map(con);
            });
            //��WebSocket�������ճɹ�����������ͼ
            app.Map("/common/socket_thumb", (con) =>
            {
                con.UseWebSockets();//����webscoket
                con.Use((ctx, n) =>
                {
                    Receiver _receive = new Receiver(ctx, "imgdata/origin");
                    _receive.OnSuccess += (data) =>
                    {
                        //�����ļ��ɹ����Զ���������ͼ
                        // ��ͼ
                        ThumbnailHandle _thumb = new ThumbnailHandle(data, "big", 920);
                        _thumb.AutoHandle();
                        string big = _thumb.GetRelativeName();

                        //Сͼ
                        _thumb.Width = 320;
                        _thumb.Folder = "small";
                        _thumb.AutoHandle();
                        string small = _thumb.GetRelativeName();

                        data.Data = new { big = big, small = small };

                        //�˴�������Ҫ������£�ִ�����ݿ����
                    };
                    return _receive.DoWork();
                });
            });


            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
