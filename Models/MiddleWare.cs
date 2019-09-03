using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuiteACWeb_Api.Models.SolControl;
namespace SuiteACWeb_Api.Models
{
    public class Middleware
    {
        private readonly RequestDelegate _next;
        private SolControl_DbContext solControl_db;
        public Middleware(RequestDelegate next, SolControl_DbContext masterDevContext)
        {
            _next = next;
            solControl_db = masterDevContext;
        }
        public async Task Invoke(HttpContext context, SuiteACWeb_Api_DbContext suiteCFDI_DBContext)
        {
            try
            {
                //string authHeader = context.Request.Headers["Authorization"];
                //if (authHeader != null && authHeader.StartsWith("Basic"))
                //{
                //    //Extract credentials    
                //    string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                //    Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                //    var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
                //    int seperatorIndex = usernamePassword.IndexOf(':');
                //    var username = usernamePassword.Substring(0, 9);
                //    var password = usernamePassword.Substring(seperatorIndex + 1);
                //    if (true) //check if your credentials are valid    
                //    {
                //        MasterDbContext.ConnectionString = "connection"; //_masterContext.Retrive Your subscriber connection string here    
                //        if (string.IsNullOrEmpty(MasterDbContext.ConnectionString))
                //        {
                //            //no authorization header    
                //            context.Response.StatusCode = 401; //Unauthorized    
                //            return;
                //        }
                //        await _next.Invoke(context);
                //    }
                //    else
                //    {
                //        context.Response.StatusCode = 401; //Unauthorized    
                //        return;
                //    }
                //}
                //else
                //{
                //    // no authorization header    
                //    context.Response.StatusCode = 401; //Unauthorized    
                //    return;
                //}

                //Identificar el Tenant
                string Header = context.Request.Headers["Postman-Token"];
                HttpRequest request = context.Request;
                string path = request.Path;
                string[] parts = path.Split('/');
                string lic = parts[1];
                string conn = @"Server=|server|;Database=|db|;Trusted_Connection=False;user id=|userId|;password=|userPsw|;ConnectRetryCount=0";
                Bitacora bit = new Bitacora();

                Instancias inst = solControl_db.Instancias.Where(a => a.Licencia == lic).FirstOrDefault();
                if (inst == null)
                {
                    //no tenant header    
                    bit.Descripcion = string.Format("Recuperar la instancia con Licencia '{0}'", lic);
                    bit.Evento = "API01";
                    bit.Fecha = DateTime.Now;
                    bit.Mensaje = string.Format("Instancia null. Licencia: {0}", lic);
                    bit.Tipo = "I";
                    bit.Usuario = "System";
                    solControl_db.Add(bit);
                    solControl_db.SaveChanges();

                    context.Response.StatusCode = 401; //Not Found    
                    return;
                    //conn = @"Server=mssql.consulteam.mx;Database=scfdi_plenty;Trusted_Connection=False;user id=scfdi_plenty_admin;password=1z2x3c.CC;ConnectRetryCount=0";
                    //SuiteCFDI_DbContext.ConnectionString = conn;
                    //await _next.Invoke(context);

                }
                //Construir Connection
                Dbs db = solControl_db.Dbs.Find(inst.IdDb);
                if (db == null)
                {
                    //Base de datos no enciontrada    
                    bit.Descripcion = string.Format("Recuperar la base de datos '{0}'", inst.IdDb);
                    bit.Evento = "API02";
                    bit.Fecha = DateTime.Now;
                    bit.Mensaje = string.Format("IdDb null. Licencia: {0}, DB: {1}", lic, inst.IdDb);
                    bit.Tipo = "I";
                    bit.Usuario = "System";
                    solControl_db.Add(bit);
                    solControl_db.SaveChanges();

                    context.Response.StatusCode = 402; //Not Found    
                    return;

                    //conn = @"Server=mssql.consulteam.mx;Database=scfdi_plenty;Trusted_Connection=False;user id=scfdi_plenty_admin;password=1z2x3c.CC;ConnectRetryCount=0";
                    //SuiteCFDI_DbContext.ConnectionString = conn;
                    //await _next.Invoke(context);

                }
                conn = conn.Replace("|server|", db.Server);
                conn = conn.Replace("|db|", db.IdDb);
                conn = conn.Replace("|userId|", db.Usuario);
                conn = conn.Replace("|userPsw|", db.Psw);

                //string conn = string.Empty;
                //if (tenant == "plenty")
                //    conn = @"Server=mssql.consulteam.mx;Database=scfdi_plenty;Trusted_Connection=False;user id=scfdi_plenty_admin;password=1z2x3c.CC;ConnectRetryCount=0";
                //if (tenant == "cteam")
                //    conn = @"Server=mssql.consulteam.mx;Database=scfdi_cteam;Trusted_Connection=False;user id=scfdi_cteam_admin;password=4f5g6h.HH;ConnectRetryCount=0";

                //MasterDbContext.TenantName = tenant;
                SuiteCFDI_DbContext.ConnectionString = conn;

                bit.Descripcion = string.Format("Establecer connection string para Licencia '{0}'", lic);
                bit.Evento = "API00";
                bit.Fecha = DateTime.Now;
                bit.Mensaje = string.Format("Connection String '{0}'", conn);
                bit.Tipo = "I";
                bit.Usuario = "System";
                solControl_db.Add(bit);
                solControl_db.SaveChanges();


                await _next.Invoke(context);
            }
            catch (Exception e)
            {
                // no authorization header
                e.ToString();
                context.Response.StatusCode = 400;
                Bitacora bit = new Bitacora();
                bit.Descripcion = string.Format("Excepción en 'MiddleWare '{0}'", "");
                bit.Evento = "API00";
                bit.Fecha = DateTime.Now;
                bit.Mensaje = string.Format("Error: {0}", e.ToString());
                bit.Tipo = "E";
                bit.Usuario = "System";
                solControl_db.Add(bit);
                solControl_db.SaveChanges();

                return;
            }
        }
    }
}
