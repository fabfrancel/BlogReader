# BlogReader
<h3>Server Side</h3>
<p>This is a study project that implements, on the server side, an .NET RESTful API (only the GET method), wich reads a list url feed/rss from a text file (.txt), gets the last post and return the appropriated JSON.</p> 
<h4>Important resources used:</h4>
  <ul>
    <li>SyndicationFeed from package System.ServiceModel.Syndication</li>
    <li>JsonResult from package Microsoft.AspNetCore.Mvc</li>
  </ul>
  <p>Is important to note in the code the differents ways of handling exceptions in each method.</p>
  <h3>Client Side</h3>
<p>On the client side, we create a Angular project to consum the service. </p>
<h4>Highlights:</h4>
<ul>
  <li>DomSanitizer that helps preventing Cross Site Scripting Security bugs (XSS) by sanitizing values to be safe to use in the different DOM contexts.</li>
  <li>firstValueFrom from RxJs library instead the deprecated subscribe function. The firstValueFrom converts an observable to a promise by subscribing to the observable, and returning a promise that will resolve as soon as the first value arrives from the observable. The subscription will then be closed.</li>
</ul>
 
