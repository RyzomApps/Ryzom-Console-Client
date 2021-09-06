using System.IO;
using System.Net.Http;
using System.Text;

namespace RCC
{
    public class Login
    {

        static readonly HttpClient client = new HttpClient();

        public Login()
        {
            // pGEBLog->setInputString(LoginLogin);
            // pGEBPwd->setInputString(LoginPassword);
            // CAHManager::getInstance()->runActionHandler("on_login", NULL, "");

            // LoginSM.pushEvent(CLoginStateMachine::ev_init_done);

            //string res = checkLogin(LoginLogin, LoginPassword, ClientApp, LoginCustomParameters);
        }


        //public async System.Threading.Tasks.Task<string> GetValueAsync()
        //{
        //    var client = new HttpClient();

        //    var webRequest = new HttpRequestMessage(HttpMethod.Post, "http://your-api.com")
        //    {
        //        Content = new StringContent("{ 'some': 'value' }", Encoding.UTF8, "application/json")
        //    };

        //    HttpResponseMessage response = await client.SendAsync(webRequest);

        //    string responseString = await response.Content.ReadAsStringAsync();

        //    return reader.ReadToEnd();
        //}


        public string CheckLogin(string login, string password, string clientApp, string customParameters)
        {
            //if (!client.get .sendGet(url + "?cmd=login&login=" + login + "&password=" + cryptedPassword + "&clientApplication=" + clientApp + "&cp=2" + "&lg=" + ClientCfg.LanguageCode + customParameters))
            //    return "Can't send (error code 2)";

            return null;
        }
    }


}