using IPTools.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime;

namespace WeatherApi.Controllers
{
    /// <summary>
    /// 获取城市天气
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class WeatherController(IHttpClientFactory _httpClientFactory) : ControllerBase
    {
        /// <summary>
        /// 获取当前时间
        /// </summary>
        /// <returns>当前时间</returns>
        [HttpGet]
        public string GetCurrentDate()
        {
            return DateTime.Now.ToString("MM/dd");
        }

        /// <summary>
        /// 获取当前城市信息
        /// </summary>
        /// <returns>当前城市信息</returns>
        [HttpGet]
        public async Task<IpInfo> GetLocation()
        {
            var httpClient = _httpClientFactory.CreateClient();
            IpData ipInfo = await httpClient.GetFromJsonAsync<IpData>("https://ipinfo.io/json");
            var ipinfo = IpTool.Search(ipInfo!.ip);
            return ipinfo;
        }

        /// <summary>
        /// 获取天气信息
        /// </summary>
        /// <param name="region">省份</param>
        /// <param name="city">城市</param>
        /// <param name="currentDate">日期(格式：月份/日期)</param>
        /// <returns>天气信息</returns>
        [HttpGet]
        public async Task<string> GetCurrentWeather(string region, string city, string currentDate)
        {
            var httpClient = _httpClientFactory.CreateClient();
            WeatherRoot weatherRoot = await httpClient.GetFromJsonAsync<WeatherRoot>($"https://cn.apihz.cn/api/tianqi/tqybmoji15.php?id=88888888&key=88888888&sheng={region!}&place={city!}")!;
            DataItem today = weatherRoot!.data!.FirstOrDefault(i => i.week2 == currentDate)!;
            return $"{today!.week2} {today.week1},天气{today.wea1}转{today.wea2}。最高气温{today.wendu1}摄氏度,最低气温{today.wendu2}摄氏度。";
        }
    }
}

public class IpData
{
    public string ip { get; set; }
    public string city { get; set; }
    public string region { get; set; }
    public string country { get; set; }
    public string loc { get; set; }
    public string org { get; set; }
    public string postal { get; set; }
    public string timezone { get; set; }
    public string readme { get; set; }
}

public class DataItem
{
    public string week1 { get; set; }
    public string week2 { get; set; }
    public string wea1 { get; set; }
    public string wea2 { get; set; }
    public string wendu1 { get; set; }
    public string wendu2 { get; set; }
    public string img1 { get; set; }
    public string img2 { get; set; }
}

public class WeatherRoot
{
    public List<DataItem> data { get; set; }
    public int code { get; set; }
    public string place { get; set; }
}
