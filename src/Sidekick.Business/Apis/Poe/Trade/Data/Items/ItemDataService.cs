using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sidekick.Business.Apis.Poe.Models;
using Sidekick.Business.Apis.Poe.Parser;
using Sidekick.Business.Languages;
using Sidekick.Core.Initialization;

namespace Sidekick.Business.Apis.Poe.Trade.Data.Items
{
    public class ItemDataService : IItemDataService, IOnInit, IOnAfterInit
    {
        private readonly IPoeTradeClient poeApiClient;
        private readonly ILanguageProvider languageProvider;
        private Dictionary<string, List<ItemData>> nameAndTypePatterns;
        private string[] prefixes;

        public ItemDataService(IPoeTradeClient poeApiClient, ILanguageProvider languageProvider)
        {
            this.poeApiClient = poeApiClient;
            this.languageProvider = languageProvider;
        }

        public async Task OnInit()
        {
            nameAndTypePatterns = new Dictionary<string, List<ItemData>>();

            var categories = await poeApiClient.Fetch<ItemDataCategory>();

            FillPattern(categories[0].Entries, Category.Accessory);
            FillPattern(categories[1].Entries, Category.Armour);
            FillPattern(categories[2].Entries, Category.DivinationCard);
            FillPattern(categories[3].Entries, Category.Currency);
            FillPattern(categories[4].Entries, Category.Flask);
            FillPattern(categories[5].Entries, Category.Gem);
            FillPattern(categories[6].Entries, Category.Jewel);
            FillPattern(categories[7].Entries, Category.Map);
            FillPattern(categories[8].Entries, Category.Weapon);
            FillPattern(categories[9].Entries, Category.Leaguestone);
            FillPattern(categories[10].Entries, Category.Prophecy);
            FillPattern(categories[11].Entries, Category.ItemisedMonster);
            FillPattern(categories[12].Entries, Category.Watchstone);
        }

        public Task OnAfterInit()
        {
            prefixes = new[]
            {
                languageProvider.Language.PrefixSuperior,
                languageProvider.Language.PrefixBlighted
            };

            return Task.CompletedTask;
        }

        private void FillPattern(List<ItemData> items, Category category)
        {
            foreach (var item in items)
            {
                item.Rarity = GetRarityForCategory(category, item);
                item.Category = category;

                var key = item.Name ?? item.Type;

                if (!nameAndTypePatterns.TryGetValue(key, out var itemData))
                {
                    itemData = new List<ItemData>();
                    nameAndTypePatterns.Add(key, itemData);
                }

                itemData.Add(item);
            }
        }

        private Rarity GetRarityForCategory(Category category, ItemData item)
        {
            if (item.Flags.Unique)
            {
                return Rarity.Unique;
            }
            else if (item.Flags.Prophecy)
            {
                return Rarity.Prophecy;
            }

            return category switch
            {
                Category.DivinationCard => Rarity.DivinationCard,
                Category.Gem => Rarity.Gem,
                Category.Currency => Rarity.Currency,
                _ => Rarity.Unknown
            };
        }

        public ItemData ParseItemData(ItemSections itemSections)
        {
            var results = new List<ItemData>();

            if (nameAndTypePatterns.TryGetValue(GetLineWithoutPrefixes(itemSections.HeaderSection[1]), out var itemData))
            {
                results.AddRange(itemData);
            }
            else if (itemSections.HeaderSection.Length > 2 && nameAndTypePatterns.TryGetValue(GetLineWithoutPrefixes(itemSections.HeaderSection[2]), out itemData))
            {
                results.AddRange(itemData);
            }

            if(results.Any(item => item.Rarity == Rarity.Gem)
                && itemSections.TryGetVaalGemName(out var vaalGemName)
                && nameAndTypePatterns.TryGetValue(vaalGemName, out itemData))
            {
                // If we find a Vaal gem, we don't care about other matches
                results.Clear();
                results.Add(itemData.First());
            }

            return results
                .OrderBy(x => x.Rarity == Rarity.Unique ? 0 : 1)
                .ThenBy(x => x.Rarity == Rarity.Unknown ? 0 : 1)
                .FirstOrDefault();
        }

        private string GetLineWithoutPrefixes(string line)
        {
            foreach (var prefix in prefixes)
            {
                line = line.Replace(prefix, "");
            }

            return line;
        }
    }
}
