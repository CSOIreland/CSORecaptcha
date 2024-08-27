using Newtonsoft.Json;

namespace CSO.Recaptcha
{
    /// <summary>
    /// Handle the Google ReCaptcha to validate human inputs in forms
    /// </summary>
    public static class ReCAPTCHA
    {
        #region Properties
        #endregion

        #region Methods
        /// <summary>
        /// Check if the ReCaptcha is enabled
        /// </summary>
        /// <param name="configDict"></param>
        /// <returns></returns>
        private static bool IsEnabled(IDictionary<string, string> configDict)
        {
            /// <summary>
            /// Flag to indicate if ReCAPTCHA is enabled
            /// </summary>
           bool API_RECAPTCHA_ENABLED = Convert.ToBoolean(configDict["API_RECAPTCHA_ENABLED"]);

            if (API_RECAPTCHA_ENABLED)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Validate the encoded response against the Google server
        /// </summary>
        /// <param name="encodedResponse"></param>
        /// <param name="configDict"></param>
        /// <param name="LogObject"></param>
        /// <returns></returns>
        public static bool Validate(string encodedResponse, IDictionary<string, string> configDict, dynamic LogObject)
        {
            /// <summary>
            /// Flag to indicate if ReCAPTCHA is enabled
            /// </summary>
            bool API_RECAPTCHA_ENABLED = Convert.ToBoolean(configDict["API_RECAPTCHA_ENABLED"]);

            /// <summary>
            /// URL
            /// </summary>
            string API_RECAPTCHA_URL = configDict["API_RECAPTCHA_URL"];

            /// <summary>
            /// privates key
            /// </summary>
            string API_RECAPTCHA_PRIVATE_KEY = configDict["API_RECAPTCHA_PRIVATE_KEY"];



            LogObject.Info("ReCAPTCHA Enabled: " + API_RECAPTCHA_ENABLED);
            LogObject.Info("ReCAPTCHA URL: " + API_RECAPTCHA_URL);
            LogObject.Info("ReCAPTCHA Private Key: ********"); // Hide API_RECAPTCHA_PRIVATE_KEY from logs

            // Skip the validation if not enabled
            if (!IsEnabled(configDict))
                return true;

            try
            {
                // Validate the response against the server
                var client = new HttpClient();

                var requestString = string.Format(API_RECAPTCHA_URL, API_RECAPTCHA_PRIVATE_KEY, encodedResponse);
                string responseString = "";
                
                using (HttpResponseMessage response = client.GetAsync(requestString).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        responseString = content.ReadAsStringAsync().Result;
                    }
                }



                //   var responseObject = Utility.JsonDeserialize_IgnoreLoopingReference<JObject>(responseString);
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None }); 
                var responseSuccess = (string)responseObject["success"];

                LogObject.Info("Server Response: " + responseString);

                if (responseSuccess.ToLowerInvariant() == "true")
                {
                    // All good and valid
                    LogObject.Info("Valid Encoded Response: " + encodedResponse);
                    return true;
                }
                else
                {
                    // Something went wrong
                    LogObject.Info("Invalid Encoded Response: " + encodedResponse);
                    return false;
                }
            }
            catch (Exception e)
            {
                LogObject.Fatal(e);
                return false;
            }
        }

        #endregion
    }
}
