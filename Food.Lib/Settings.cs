/**
Microsoft Sample Application Usage License

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

1. Definitions
The terms “reproduce,” “reproduction,” “derivative works,” and “distribution” have the same meaning here as under U.S. copyright law.
A “contribution” is the original software, or any additions or changes to the software.
A “contributor” is any person that distributes its contribution under this license.
“Licensed patents” are a contributor’s patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors’ name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
(E) The software is licensed “as-is.” You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
(F) Platform Limitation- The licenses granted in sections 2(A) & 2(B) extend only to the software or derivative works that (1) runs on a Microsoft Windows operating system product, and (2) operates with Microsoft Bing services.
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Helpers;

namespace Microsoft.DPE.ReferenceApps.Food.Lib
{
    public static class Settings
    {
        // hub
        public static int NearMeMaxDistanceThresholdInMiles = 25;
        public static int TrendingMinimumReviewAgeInDays = 21;
        public static double TrendingMinimumRating = 3.0d;
        public static int RecentlyReviewedMinimumInDays = 14;
        public static int LocationMovementThresholdInMiles = 1;
        public static int MaxPinsPerCluster = 5;
        public static int PinClusterPixelRadius = 125;
        public static bool UseMockData = true;
        public static bool ShowJSONPanel = false;

        // yelp
        // See http://www.yelp.com/developers/documentation/v2/overview 
        // for info on getting keys
        public static string YelpConsumerKey = "YOURYELPCONSUMERKEY";
        public static string YelpConsumerSecret = "YOURYELPCONSUMERSECRET";
        public static string YelpToken = "YOURYELPTOKEN";
        public static string YelpTokenSecret = "YOURYELPTOKENSECRET";
        public static string YelpV1Key = "YOURYELPV1KEY";

        // factual
        // See https://www.factual.com/api-keys/request
        // for info on getting keys
        public static string FactualConsumerKey = "YOURFACTUALCONSUMERKEY";
        public static string FactualConsumerSecret = "YOURFACTUALCONSUMERSECRET";

        // openmenu
        // See http://openmenu.com/account/
        // for info on getting keys
        public static string OpenMenuKey = "YOUROPENMENUKEY";

        // Bing
        // See http://msdn.microsoft.com/en-us/library/dd900818.aspx for Bing Search
        // See http://msdn.microsoft.com/en-us/library/ff428642.aspx for Bing Maps
        public static string BingMapsKey = "YOURBINGMAPSKEY";
        public static string BingSearchKey = "YOURBINGSEARCHKEY";
        public static string BingSearchCustomerID = "YOURBINGSEARCHCUSTOMERID";
        public static int BingMapsInitialZoomLevel = 10;

        // UserSettings CacheKeys
        public static string FavoritesCacheKey = "UserSettings_Favorites";
        public static string NearMeMaxDistanceCacheKey = "UserSettings_NearMeMaxDistance";

        public static int PositiveAggregateRatingThreshold = 70;
    }
}
