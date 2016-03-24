﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace Microsoft.DPE.ReferenceApps.Common.OAuthLib
{
    public enum AuthType { ByHeader, ByURL };
    public sealed class OAuthManager
    {
        /// <summary>
        ///   The default public constructor.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Initializes various fields to default values.
        ///   </para>
        /// </remarks>
        public OAuthManager()
        {
            _random = new Random();
            _params = new Dictionary<String, String>();
            _params["callback"] = "oob"; // presume "desktop" consumer
            _params["consumer_key"] = "";
            _params["consumer_secret"] = "";
            _params["timestamp"] = GenerateTimeStamp();
            _params["nonce"] = GenerateNonce();
            _params["signature_method"] = "HMAC-SHA1";
            _params["signature"] = "";
            _params["token"] = "";
            _params["token_secret"] = "";
            _params["version"] = "1.0";
        }

        /// <summary>
        ///   The constructor to use when using OAuth when you already
        ///   have an OAuth access token.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The parameters for this constructor all have the
        ///     meaning you would expect.  The token and tokenSecret
        ///     are set in oauth_token, and oauth_token_secret.
        ///     These are *Access* tokens, obtained after a call
        ///     to AcquireAccessToken.  The application can store
        ///     those tokens and re-use them on successive runs.
        ///     For twitter at least, the access tokens never expire.
        ///   </para>
        /// </remarks>
        public OAuthManager(string consumerKey,
                       string consumerSecret,
                       string token,
                       string tokenSecret)
            : this()
        {
            _random = new Random();
            _params["consumer_key"] = consumerKey;
            _params["consumer_secret"] = consumerSecret;
            _params["token"] = token;
            _params["token_secret"] = tokenSecret;
        }

        /// <summary>
        ///   The constructor to use when using OAuth when you already
        ///   have an OAuth access token.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The parameters for this constructor all have the
        ///     meaning you would expect.  The token and tokenSecret
        ///     are set in oauth_token, and oauth_token_secret.
        ///     These are *Access* tokens, obtained after a call
        ///     to AcquireAccessToken.  The application can store
        ///     those tokens and re-use them on successive runs.
        ///     For twitter at least, the access tokens never expire.
        ///   </para>
        /// </remarks>
        public OAuthManager(string consumerKey,
                       string consumerSecret)
            : this()
        {
            _random = new Random();
            _params["consumer_key"] = consumerKey;
            _params["consumer_secret"] = consumerSecret;
        }

        /// <summary>
        ///   string indexer to get or set oauth parameter values.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Use the parameter name *without* the oauth_ prefix.
        ///     If you want to set the value for the oauth_token parameter
        ///     field in an HTTP message, then use oauth["token"].
        ///   </para>
        ///   <para>
        ///     The set of oauth param names known by this indexer includes:
        ///     callback, consumer_key, consumer_secret, timestamp, nonce,
        ///     signature_method, signature, token, token_secret, and version.
        ///   </para>
        ///   <para>
        ///     If you try setting a parameter with a name that is not known,
        ///     the setter will throw.  You cannot add new oauth parameters
        ///     using the setter on this indexer.
        ///   </para>
        /// </remarks>
        public string GetParam(string ix)
        {
            if (_params.ContainsKey(ix))
                return _params[ix];
            throw new ArgumentException(ix);
        }

        public void SetParam(string ix, string value)
        {
            if (!_params.ContainsKey(ix))
                throw new ArgumentException(ix);
            _params[ix] = value;
        }

        public string getValue(string key)
        {
                if (_params.ContainsKey(key))
                    return _params[key];
                throw new ArgumentException(key);
        }

        public void setValue(string key, string value)
        {
            if (!_params.ContainsKey(key))
                throw new ArgumentException(key);
            _params[key] = value;
        }


        /// <summary>
        /// Generate the timestamp for the signature.
        /// </summary>
        /// <returns>The timestamp, in string form.</returns>
        private string GenerateTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - _epoch;
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        ///   Renews the nonce and timestamp on the oauth parameters.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Each new request should get a new, current timestamp, and a
        ///     nonce. This helper method does both of those things. This gets
        ///     called before generating an authorization header, as for example
        ///     when the user of this class calls <see cref='AcquireRequestToken'>.
        ///   </para>
        /// </remarks>
        private void NewRequest()
        {
            _params["nonce"] = GenerateNonce();
            _params["timestamp"] = GenerateTimeStamp();
        }

        /// <summary>
        /// Generate an oauth nonce.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     According to RFC 5849, A nonce is a random string,
        ///     uniquely generated by the client to allow the server to
        ///     verify that a request has never been made before and
        ///     helps prevent replay attacks when requests are made over
        ///     a non-secure channel.  The nonce value MUST be unique
        ///     across all requests with the same timestamp, client
        ///     credentials, and token combinations.
        ///   </para>
        ///   <para>
        ///     One way to implement the nonce is just to use a
        ///     monotonically-increasing integer value.  It starts at zero and
        ///     increases by 1 for each new request or signature generated.
        ///     Keep in mind the nonce needs to be unique only for a given
        ///     timestamp!  So if your app makes less than one request per
        ///     second, then using a static nonce of "0" will work.
        ///   </para>
        ///   <para>
        ///     Most oauth nonce generation routines are waaaaay over-engineered,
        ///     and this one is no exception.
        ///   </para>
        /// </remarks>
        /// <returns>the nonce</returns>
        private string GenerateNonce()
        {
            return _random.Next(123400, 9999999).ToString();
            //return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Internal function to extract from a URL all query string
        /// parameters that are not related to oauth - in other words all
        /// parameters not begining with "oauth_".
        /// </summary>
        ///
        /// <remarks>
        ///   <para>
        ///     For example, given a url like http://foo?a=7&guff, the
        ///     returned value will be a Dictionary of string-to-string
        ///     relations.  There will be 2 entries in the Dictionary: "a"=>7,
        ///     and "guff"=>"".
        ///   </para>
        /// </remarks>
        ///
        /// <param name="queryString">The query string part of the Url</param>
        ///
        /// <returns>A Dictionary containing the set of
        /// parameter names and associated values</returns>
        private Dictionary<String, String> ExtractQueryParameters(string queryString)
        {
            if (queryString.StartsWith("?"))
                queryString = queryString.Remove(0, 1);

            var result = new Dictionary<String, String>();

            if (string.IsNullOrEmpty(queryString))
                return result;

            foreach (string s in queryString.Split('&'))
            {
                if (!string.IsNullOrEmpty(s) && !s.StartsWith("oauth_"))
                {
                    if (s.IndexOf('=') > -1)
                    {
                        string[] temp = s.Split('=');
                        result.Add(temp[0], temp[1]);
                    }
                    else
                        result.Add(s, string.Empty);
                }
            }

            return result;
        }

        /// <summary>
        /// Formats the list of request parameters into string a according
        /// to the requirements of oauth. The resulting string could be used
        /// in the Authorization header of the request.
        /// </summary>
        ///
        /// <remarks>
        ///   <para>
        ///     See http://dev.twitter.com/pages/auth#intro  for some
        ///     background.  The output of this is not suitable for signing.
        ///   </para>
        ///   <para>
        ///     There are 2 formats for specifying the list of oauth
        ///     parameters in the oauth spec: one suitable for signing, and
        ///     the other suitable for use within Authorization HTTP Headers.
        ///     This method emits a string suitable for the latter.
        ///   </para>
        /// </remarks>
        ///
        /// <param name="parameters">The Dictionary of
        /// parameters. It need not be sorted.</param>
        ///
        /// <returns>a string representing the parameters</returns>
        private static string EncodeRequestParameters(ICollection<KeyValuePair<String, String>> p)
        {
            var sb = new System.Text.StringBuilder();
            foreach (KeyValuePair<String, String> item in p.OrderBy(x => x.Key))
            {
                if (!String.IsNullOrEmpty(item.Value) &&
                    !item.Key.EndsWith("secret"))
                    sb.AppendFormat("oauth_{0}=\"{1}\", ",
                                    item.Key,
                                    Uri.EscapeDataString(item.Value));
            }

            return sb.ToString().TrimEnd(' ').TrimEnd(',');
        }

        private async Task<OAuthResponse> AcquireRT(string uri, string method, AuthType authType)
        {
            NewRequest();

            // prepare the token request
            HttpWebRequest request;
            string authzHeader = GetAuthorizationHeader(uri, method);

            if (AuthType.ByHeader == authType)
            {
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Headers["Authorization"] = authzHeader;
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(GetOAuthParameterizedUri(uri));
            }

            request.Method = method;
            //System.Diagnostics.Debugger.Launch();
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var r = new OAuthResponse(reader.ReadToEnd());
                    this.SetParam("token", r.GetParam("oauth_token"));

                    // Sometimes the request_token URL gives us an access token,
                    // with no user interaction required. Eg, when prior approval
                    // has already been granted.
                    try
                    {
                        if (r.GetParam("oauth_token_secret") != null)
                            this.SetParam("token_secret", r.GetParam("oauth_token_secret"));
                    }
                    catch { }
                    return r;
                }
            }
        }

        /// <summary>
        ///   Acquire a request token, from the given URI, using the given
        ///   HTTP method.
        /// </summary>
        ///
        /// <remarks>
        ///   <para>
        ///     To use this method, first instantiate a new OAuthManager object,
        ///     then set the callback param (oauth["callback"]='oob'). After the
        ///     call returns, you should direct the user to open a browser window
        ///     to the authorization page for the OAuth-enabled service. Or,
        ///     you can automatically open that page yourself.
        ///     There should be one query param: oauth_token with the value
        ///     obtained from oauth["token"].
        ///   </para>
        ///   <para>
        ///     According to the OAuth spec, you need to do this only ONCE per
        ///     application.  In other words, the first time the application
        ///     is run.  The normal oauth workflow is:  (1) get a request token,
        ///     (2) use that to acquire an access token (which requires explicit
        ///     user approval), then (3) using that access token, invoke
        ///     protected services.  The first two steps need to be done only
        ///     once per application.
        ///   </para>
        ///   <para>
        ///     For Twitter, at least, you can cache the access tokens
        ///     indefinitely; Twitter says they never expire.  However, other
        ///     oauth services may not do the same. Also: the user may at any
        ///     time revoke his authorization for your app, in which case you
        ///     need to perform the first 2 steps again.
        ///   </para>
        /// </remarks>
        ///
        /// <example>
        ///   <para>
        ///     This example shows how to request an access token and key
        ///     from Twitter. It presumes you've already obtained a
        ///     consumer key and secret via app registration. Requesting
        ///     an access token is necessary only the first time you
        ///     contact the service. You can cache the access key and
        ///     token for subsequent runs, later.
        ///   </para>
        ///   <code>
        ///   // the URL to obtain a temporary "request token"
        ///   var rtUrl = "https://api.twitter.com/oauth/request_token";
        ///   var oauth = new OAuth.Manager();
        ///   // The consumer_{key,secret} are obtained via registration
        ///   oauth["consumer_key"] = "~~~CONSUMER_KEY~~~~";
        ///   oauth["consumer_secret"] = "~~~CONSUMER_SECRET~~~";
        ///   oauth.AcquireRequestToken(rtUrl, "POST");
        ///   var authzUrl = "https://api.twitter.com/oauth/authorize?oauth_token=" + oauth["token"];
        ///   System.Diagnostics.Process.Start(authzUrl);
        ///   // instruct the user to type in the PIN from that browser window
        ///   var pin = "...";
        ///   var atUrl = "https://api.twitter.com/oauth/access_token";
        ///   oauth.AcquireAccessToken(atUrl, "POST", pin);
        ///
        ///   // now, update twitter status using that access token
        ///   var appUrl = "http://api.twitter.com/1/statuses/update.xml?status=Hello";
        ///   var authzHeader = oauth.GenerateAuthzHeader(appUrl, "POST");
        ///   var request = (HttpWebRequest)WebRequest.Create(appUrl);
        ///   request.Method = "POST";
        ///   request.PreAuthenticate = true;
        ///   request.AllowWriteStreamBuffering = true;
        ///   request.Headers.Add("Authorization", authzHeader);
        ///
        ///   using (var response = (HttpWebResponse)request.GetResponse())
        ///   {
        ///     if (response.StatusCode != HttpStatusCode.OK)
        ///       MessageBox.Show("There's been a problem trying to tweet:" +
        ///                       Environment.NewLine +
        ///                       response.StatusDescription);
        ///   }
        ///   </code>
        /// </example>
        ///
        /// <returns>
        ///   a response object that contains the entire text of the response,
        ///   as well as extracted parameters. This method presumes the
        ///   response is query-param encoded.
        /// </returns>
        /// 
        public IAsyncOperation<OAuthResponse> AcquireRequestTokenAsync(string uri, string method, AuthType authType)
        {
            return AsyncInfo.Run((CancellationToken ct) => AcquireRT(uri, method, authType) );
        }

        private async Task<OAuthResponse> AcquireAT(string uri, string method, string pin, AuthType authType)
        {
            NewRequest();
            _params["verifier"] = pin;

            HttpWebRequest request;
            string authzHeader = GetAuthorizationHeader(uri, method);
            // prepare the token request
            if (AuthType.ByHeader == authType)
            {
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Headers["Authorization"] = authzHeader;
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(GetOAuthParameterizedUri(uri));
            }

            request.Method = method;

            using (System.Net.WebResponse response = await request.GetResponseAsync())
            {
                using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    var r = new OAuthResponse(reader.ReadToEnd());
                    this.SetParam("token", r.GetParam("oauth_token"));
                    this.SetParam("token_secret", r.GetParam("oauth_token_secret"));
                    return r;
                }
            }
        }


        /// <summary>
        ///   Acquire an access token, from the given URI, using the given
        ///   HTTP method.
        /// </summary>
        ///
        /// <remarks>
        ///   <para>
        ///     To use this method, you must first set the oauth_token to the value
        ///     of the request token.  Eg, oauth["token"] = "whatever".
        ///   </para>
        ///   <para>
        ///     According to the OAuth spec, you need to do this only ONCE per
        ///     application.  In other words, the first time the application
        ///     is run.  The normal oauth workflow is:  (1) get a request token,
        ///     (2) use that to acquire an access token (which requires explicit
        ///     user approval), then (3) using that access token, invoke
        ///     protected services.  The first two steps need to be done only
        ///     once per application.
        ///   </para>
        ///   <para>
        ///     For Twitter, at least, you can cache the access tokens
        ///     indefinitely; Twitter says they never expire.  However, other
        ///     oauth services may not do the same. Also: the user may at any
        ///     time revoke his authorization for your app, in which case you
        ///     need to perform the first 2 steps again.
        ///   </para>
        /// </remarks>
        ///
        /// <returns>
        ///   a response object that contains the entire text of the response,
        ///   as well as extracted parameters. This method presumes the
        ///   response is query-param encoded.
        /// </returns>
        public IAsyncOperation<OAuthResponse> AcquireAccessTokenAsync(string uri, string method, string pin, AuthType authType)
        {
            return AsyncInfo.Run((CancellationToken ct) => AcquireAT(uri, method, pin, authType));
        }

        public string GetOAuthParameterizedUri(string url)
        {
            var uri = new Uri(url);
            var normUrl = string.Format("{0}://{1}", uri.Scheme, uri.Host);
            if (!((uri.Scheme == "http" && uri.Port == 80) ||
                  (uri.Scheme == "https" && uri.Port == 443)))
                normUrl += ":" + uri.Port;

            normUrl += uri.AbsolutePath;

            // the sigbase starts with the method and the encoded URI
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}?", normUrl);

            // the parameters follow - all oauth params plus any params on
            // the uri
            // each uri may have a distinct set of query params
            var p = ExtractQueryParameters(uri.Query);
            // add all non-empty params to the "current" params
            foreach (var p1 in this._params)
            {
                // Exclude all oauth params that are secret or
                // signatures; any secrets should be kept to ourselves,
                // and any existing signature will be invalid.
                if (!String.IsNullOrEmpty(this._params[p1.Key]) &&
                    !p1.Key.EndsWith("_secret"))
                    p.Add("oauth_" + p1.Key, Uri.EscapeDataString(p1.Value));
            }

            // concat+format all those params
            var sb1 = new System.Text.StringBuilder();
            foreach (KeyValuePair<String, String> item in p.OrderBy(x => x.Key))
            {
                // even "empty" params need to be encoded this way.
                sb1.AppendFormat("{0}={1}&", item.Key, item.Value);
            }

            // append the UrlEncoded version of that string to the sigbase
            sb.Append(sb1.ToString().TrimEnd('&'));

            return sb.ToString();
        }

        /// <summary>
        ///   Generate a string to be used in an Authorization header in
        ///   an HTTP request.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This method assembles the available oauth_ parameters that
        ///     have been set in the Dictionary in this instance, produces
        ///     the signature base (As described by the OAuth spec, RFC 5849),
        ///     signs it, then re-formats the oauth_ parameters into the
        ///     appropriate form, including the oauth_signature value, and
        ///     returns the result.
        ///   </para>
        ///   <para>
        ///     If you pass in a non-null, non-empty realm, this method will
        ///     include the realm='foo' clause in the Authorization header.
        ///   </para>
        /// </remarks>
        ///
        /// <seealso cref='GenerateAuthzHeader'>
        public string GenerateCredsHeader(string uri, string method, string realm)
        {
            NewRequest();
            var authzHeader = GetAuthorizationHeader(uri, method, realm);
            return authzHeader;
        }


        /// <summary>
        ///   Generate a string to be used in an Authorization header in
        ///   an HTTP request.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This method assembles the available oauth_ parameters that
        ///     have been set in the Dictionary in this instance, produces
        ///     the signature base (As described by the OAuth spec, RFC 5849),
        ///     signs it, then re-formats the oauth_ parameters into the
        ///     appropriate form, including the oauth_signature value, and
        ///     returns the result.
        ///   </para>
        /// </remarks>
        ///
        /// <example>
        ///   <para>
        ///     This example shows how to update the Twitter status
        ///     using the stored consumer key and secret, and a previously
        ///     obtained access token and secret.
        ///   </para>
        ///   <code>
        ///   var oauth = new OAuth.Manager();
        ///   oauth["consumer_key"]    = "~~ your stored consumer key ~~";
        ///   oauth["consumer_secret"] = "~~ your stored consumer secret ~~";
        ///   oauth["token"]           = "~~ your stored access token ~~";
        ///   oauth["token_secret"]    = "~~ your stored access secret ~~";
        ///   var appUrl = "http://api.twitter.com/1/statuses/update.xml?status=Hello";
        ///   var authzHeader = oauth.GenerateAuthzHeader(appUrl, "POST");
        ///   var request = (HttpWebRequest)WebRequest.Create(appUrl);
        ///   request.Method = "POST";
        ///   request.PreAuthenticate = true;
        ///   request.AllowWriteStreamBuffering = true;
        ///   request.Headers.Add("Authorization", authzHeader);
        ///
        ///   using (var response = (HttpWebResponse)request.GetResponse())
        ///   {
        ///     if (response.StatusCode != HttpStatusCode.OK)
        ///       MessageBox.Show("There's been a problem trying to tweet:" +
        ///                       Environment.NewLine +
        ///                       response.StatusDescription);
        ///   }
        ///   </code>
        /// </example>
        /// <seealso cref='GenerateCredsHeader'>
        public string GenerateAuthzHeader(string uri, string method)
        {
            NewRequest();
            var authzHeader = GetAuthorizationHeader(uri, method, null);
            return authzHeader;
        }

        private string GetAuthorizationHeader(string uri, string method)
        {
            return GetAuthorizationHeader(uri, method, null);
        }

        private string GetAuthorizationHeader(string uri, string method, string realm)
        {
            if (string.IsNullOrEmpty(this._params["consumer_key"]))
                throw new ArgumentNullException("consumer_key");

            if (string.IsNullOrEmpty(this._params["signature_method"]))
                throw new ArgumentNullException("signature_method");

            Sign(uri, method);

            var erp = EncodeRequestParameters(this._params);
            return (String.IsNullOrEmpty(realm))
                ? "OAuth " + erp
                : String.Format("OAuth realm=\"{0}\", ", realm) + erp;
        }

        private void Sign(string uri, string method)
        {
            var signatureBase = GetSignatureBase(uri, method);
            var macKey = GetCryptoKey();

            IBuffer dataToBeSigned =
                CryptographicBuffer.ConvertStringToBinary(signatureBase, BinaryStringEncoding.Utf8);

            IBuffer signatureBuffer = CryptographicEngine.Sign(macKey, dataToBeSigned);
            this.SetParam("signature", CryptographicBuffer.EncodeToBase64String(signatureBuffer));
        }

        /// <summary>
        /// Formats the list of request parameters into "signature base" string as
        /// defined by RFC 5849.  This will then be MAC'd with a suitable hash.
        /// </summary>
        private string GetSignatureBase(string url, string method)
        {
            // normalize the URI
            var uri = new Uri(url);
            var normUrl = string.Format("{0}://{1}", uri.Scheme, uri.Host);
            if (!((uri.Scheme == "http" && uri.Port == 80) ||
                  (uri.Scheme == "https" && uri.Port == 443)))
                normUrl += ":" + uri.Port;

            normUrl += uri.AbsolutePath;

            // the sigbase starts with the method and the encoded URI
            StringBuilder sb = new StringBuilder();
            sb.Append(method)
                .Append('&')
                .Append(Uri.EscapeDataString(normUrl))
                .Append('&');

            // the parameters follow - all oauth params plus any params on
            // the uri
            // each uri may have a distinct set of query params
            var p = ExtractQueryParameters(uri.Query);
            // add all non-empty params to the "current" params
            foreach (var p1 in this._params)
            {
                // Exclude all oauth params that are secret or
                // signatures; any secrets should be kept to ourselves,
                // and any existing signature will be invalid.
                if (!String.IsNullOrEmpty(this._params[p1.Key]) &&
                    !p1.Key.EndsWith("_secret") &&
                    !p1.Key.EndsWith("signature"))
                    p.Add("oauth_" + p1.Key, p1.Value);
            }

            // concat+format all those params
            var sb1 = new System.Text.StringBuilder();
            foreach (KeyValuePair<String, String> item in p.OrderBy(x => x.Key))
            {
                // even "empty" params need to be encoded this way.
                sb1.AppendFormat("{0}={1}&", item.Key, item.Value);
            }

            // append the UrlEncoded version of that string to the sigbase
            sb.Append(Uri.EscapeDataString(sb1.ToString().TrimEnd('&')));
            string result = sb.ToString();
            return result;
        }

        private CryptographicKey GetCryptoKey()
        {
            if (this.GetParam("signature_method") != "HMAC-SHA1")
                throw new NotImplementedException();

            MacAlgorithmProvider hmacSha1Provider =
                MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");

            IBuffer keyMaterial =
                CryptographicBuffer.ConvertStringToBinary(
                    string.Format(
                        "{0}&{1}",
                        this.GetParam("consumer_secret"),
                        this.GetParam("token_secret")),
                    BinaryStringEncoding.Utf8);

            return hmacSha1Provider.CreateKey(keyMaterial);
        }

        private Random _random;
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private Dictionary<String, String> _params;
    }


    /// <summary>
    ///   A class to hold an OAuth response message.
    /// </summary>
    public sealed class OAuthResponse
    {
        /// <summary>
        ///   All of the text in the response. This is useful if the app wants
        ///   to do its own parsing.
        /// </summary>
        public string AllText { get; set; }
        private Dictionary<String, String> _params;

        /// <summary>
        ///   a Dictionary of response parameters.
        /// </summary>
        public string GetParam(string ix)
        {
            return _params[ix];
        }

        public string getKey(string ix)
        {
            if (_params.ContainsKey(ix))
                return _params[ix];
            throw new ArgumentException(ix);
        }

        public void setKey(string ix, string value)
        {
            if (!_params.ContainsKey(ix))
                throw new ArgumentException(ix);
            _params[ix] = value;
        }

        public OAuthResponse(string alltext)
        {
            AllText = alltext;
            _params = new Dictionary<String, String>();
            var kvpairs = alltext.Split('&');
            foreach (var pair in kvpairs)
            {
                var kv = pair.Split('=');
                _params.Add(kv[0], kv[1]);
            }
            // expected keys:
            //   oauth_token, oauth_token_secret, user_id, screen_name, etc
        }
    }
}

