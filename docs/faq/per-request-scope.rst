==============================================
How do I work with per-request lifetime scope?
==============================================

Note for doc writing: This should cover all the common per-request scope questions we run into. We may need to break it into smaller pages if it gets too big.

- How do I deal with the "No AutofacWebRequest lifetime scope" exception?
- How do I simulate per-request scope in unit tests?
- Why isn't Web API filter property injection using the request lifetime scope? (Cross link this with the Web API integration page, too.)

  * https://github.com/autofac/Autofac/issues/525
  * http://stackoverflow.com/questions/23659108/webapi-autofac-system-web-http-filters-actionfilterattribute-instance-per-requ/23679744#23679744
  * https://github.com/autofac/Autofac/issues/452

- Cross link with the instance scope page.