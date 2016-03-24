using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Helpers
{
    public static class FavoritesHelper
    {
        private const string CACHEKEY = "Favorites";
        public static event EventHandler FavoritesChanged;
        private static void RaiseFavoritesChanged()
        {
            m_List = null;
            if (FavoritesChanged != null)
                FavoritesChanged(null, EventArgs.Empty);
        }

        public async static void Clear()
        {
            await Save(new List<Restaurant>());
            RaiseFavoritesChanged();
        }

        public async static Task AddAsync(IEnumerable<Restaurant> restaurants)
        {
            foreach (var item in restaurants)
                await AddAsync(item, false);
            RaiseFavoritesChanged();
        }

        public async static Task AddAsync(Restaurant restaurant, bool raiseChanged = true)
        {
            var _Favorites = await List();
            _Favorites.RemoveAll(x => x.Key == restaurant.Key);
            _Favorites.Add(restaurant);
            await Save(_Favorites);
            if (raiseChanged)
                RaiseFavoritesChanged();
        }

        public async static Task RemoveAsync(IEnumerable<Restaurant> restaurants)
        {
            foreach (var item in restaurants)
                await RemoveAsync(item, false);
            RaiseFavoritesChanged();
        }

        public async static Task RemoveAsync(Restaurant restaurant, bool raiseChanged = true)
        {
            var _Favorites = await List();
            _Favorites.RemoveAll(x => x.Key == restaurant.Key);
            await Save(_Favorites);
            if (raiseChanged)
                RaiseFavoritesChanged();
        }

        private static async Task Save(IEnumerable<Restaurant> list)
        {
            await StorageHelper.WriteFileAsync(CACHEKEY, list,
                StorageHelper.StorageStrategies.Roaming);
        }

        private static List<Restaurant> m_List;
        public async static Task<List<Restaurant>> List()
        {
            if (m_List == null) // first read
                m_List = await StorageHelper.ReadFileAsync<List<Restaurant>>(CACHEKEY, StorageHelper.StorageStrategies.Roaming);
            return m_List ?? new List<Restaurant>();
        }
    }
}
