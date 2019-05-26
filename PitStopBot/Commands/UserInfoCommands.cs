﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PitStopBot.Objects;
using PitStopBot.Utils;
using Nethereum.Util;
using System.Collections.Generic;
using System.Linq;

namespace PitStopBot.Commands {
	[Group("user")]
	public class UserInfo : ModuleBase {

		private EmbedBuilder MyEmbedBuilder = new EmbedBuilder();
		private EmbedFieldBuilder MyEmbedField = new EmbedFieldBuilder();
		private string emptyAddress = "0x0000000000000000000000000000000000000000";
		private string ensUrl = "https://manager.ens.domains/name/";

		UserInfoUtils userUtils = new UserInfoUtils();
		public UserInfo() {
		}

		private async Task<string> GetFormattedAddress(string addressInput) {
			string addressToFormat = null;
			if (addressInput.Contains(".eth")) {
				EnsUtils ensUtil = new EnsUtils();
				var ens = await ensUtil.GetENS(addressInput);
				addressToFormat = ens.address;
			} else {
				addressToFormat = addressInput;
			}
			return new AddressUtil().ConvertToChecksumAddress(addressToFormat);
		}

		[Command("rarities"), Summary("returns the parts count")]
		public async Task GetRarities([Summary("User's eth adress")] string addressInput) {
			var address = await GetFormattedAddress(addressInput);
			if (address.Equals(emptyAddress)) {
				await ReplyAsync($"Invalid ENS. Please make sure to set the resolver and then your address in the domains.\n" +
					$"Do so here: {ensUrl}{addressInput}");
				return;
			}
			Inventory inv = await userUtils.GetInventory(address);
			var parts = inv.parts;
			var rarityList = new List<string>();
			foreach (var p in parts) {
				rarityList.Add(p.details.rarity);
			}
			MyEmbedBuilder.WithTitle("Part Rarity Distribution");
			MyEmbedBuilder.WithColor(Color.Blue);
			var order = "CREL"; //common, rare, epic, legendary
			var orderDict = order.Select((c, i) => new { Letter = c, Order = i })
								 .ToDictionary(o => o.Letter, o => o.Order);
			var rarities = rarityList.OrderBy(i =>orderDict[i[0]]).GroupBy(i => i);
			foreach (var r in rarities) {
				MyEmbedBuilder.AddField(r.Key, r.Count(), true);
			}

			await ReplyAsync(embed: MyEmbedBuilder.Build());
		}

		[Command("parts"), Summary("returns the parts count")]
		public async Task GetParts([Summary("User's eth adress")] string addressInput) {
			var address = await GetFormattedAddress(addressInput);
			int wheels = 0, bumper = 0, spoiler = 0, casing = 0;
			Inventory inv = await userUtils.GetInventory(address);
			var parts = inv.parts;
			foreach (var p in parts) {
				var type = p.details.type;
				switch (type) {
					case "wheels":
						wheels++;
						break;
					case "bumper":
						bumper++;
						break;
					case "spoiler":
						spoiler++;
						break;
					case "casing":
						casing++;
						break;
				}
			}
			MyEmbedBuilder.WithTitle("Part Type Distribution");
			MyEmbedBuilder.WithColor(Color.Green);
			MyEmbedBuilder.AddField("Wheels", wheels, true);
			MyEmbedBuilder.AddField("Front", bumper, true);
			MyEmbedBuilder.AddField("Rear", spoiler, true);
			MyEmbedBuilder.AddField("Body", casing, true);
			await ReplyAsync(embed: MyEmbedBuilder.Build());
		}
		[Command("elites"), Summary("returns the parts count")]
		public async Task GetEliteCount([Summary("User's eth adress")] string addressInput) {
			var address = await GetFormattedAddress(addressInput);
			Inventory inv = await userUtils.GetInventory(address);

			MyEmbedBuilder.WithTitle("Elite Parts:");
			MyEmbedBuilder.WithColor(Color.LightGrey);
			MyEmbedBuilder.AddField("Count", inv.parts.Count(i => i.details.isElite), true);
			await ReplyAsync(embed: MyEmbedBuilder.Build());
		}

		[Command("brands"), Summary("returns the brand count")]
		public async Task GetBrands([Summary("User's eth adress")] string addressInput) {
			var address = await GetFormattedAddress(addressInput);
			Inventory inv = await userUtils.GetInventory(address);
			var parts = inv.parts;
			var brandList = new List<string>();
			foreach (var p in parts) {
				brandList.Add(p.details.brand);
			}
			MyEmbedBuilder.WithTitle("Brand Distribution");
			MyEmbedBuilder.WithColor(Color.DarkMagenta);
			var brands = brandList.GroupBy(i => i).OrderBy(i => i.Key);
			foreach(var b in brands) {
				MyEmbedBuilder.AddField(b.Key, b.Count(), true);
			}
			await ReplyAsync(embed: MyEmbedBuilder.Build());
		}
	}
}
