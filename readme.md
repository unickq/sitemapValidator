# SV - sitemap validator helper lib

Parses sitemap links and generated [Allure](https://github.com/allure-framework)/[NUnit](https://github.com/nunit/docs/wiki/Test-Result-XML-Format) reports with status codes.
### Usage:
`sv.exe --url https://www.sitemaps.org/sitemap.xml`
or
`dotnet sv.dll --url https://www.sitemaps.org/sitemap.xml`

### Parameters:
`-u|--url <URL>` - sitemap url to validate.
`-n|--count <N>` - Max urls to test. Picks random number of urls from parsed xml.
`-w|--workers` - nUnit workers count.
`--teamcity` - nUnit TeamCity listener.
`--allure` - generates Allure report in results dir. Make sure you have installed [Allure binary correctly](https://docs.qameta.io/allure/#_installing_a_commandline). 

### Example:
`sv --url https://www.sitemaps.org/sitemap.xml -n 10 --allure`
![Allure report](https://raw.githubusercontent.com/unickq/sitemapValidator/master/reportImg.png)
