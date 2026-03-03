<h1 align="center">JWT Authentication with Authenticator Apps</h1>
<h3>This project was made with .Net 10v</h3>
<h1>About</h1>
<h3>This project was created to work with the JWT and Authenticator App</h3>
<h1>How to start</h1>
<h3>Git clone</h3>
<pre><code>git clone https://github.com/ALAN-MS-SU/Authenticator-JWT.git</code></pre>
<h2>Create the appsettings.json file and fill in all environment variables</h2>
<pre><code>{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "Redis": "",
    "Postgres": ""
  },
  "JWT": {
    "Key": "",
    "Issuer": "",
    "Audience": "",
    "Name": "",
    "Expires": 0,
    "HttpOnly": "",
    "Secure": "",
    "SameSite": "",
    "IsEssential": ""
  },
  "TOTP": {
    "Length": 0,
    "Issuer": "",
    "Limit": 0,
    "Timeout": 0,
    "ATPrefix": "",
    "SIPrefix": "",
    "PPrefix": ""
  },
  "Cors": {
    "Front": ""
  }
}</code></pre>
<h3>To start</h3>
<h3>It's necessary to have <a href="https://dotnet.microsoft.com/pt-br/download/dotnet/10.0">.Net 10v</a> installed on your computer</h3>
<pre><code>dotnet run</code></pre>
