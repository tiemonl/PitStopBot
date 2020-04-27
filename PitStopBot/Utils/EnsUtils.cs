﻿using System.Threading.Tasks;
using Nethereum.ENS;
using Nethereum.ENS.ENSRegistry.ContractDefinition;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;

namespace PitStopBot.Utils {
    public class EnsUtils {
        public async Task<string> GetEnsAddress(string ensName) {
            var ensUtil = new EnsUtil();
            var contract = "0x314159265dD8dbb310642f98f50C066173C1259b"; //ENS contract address
            var web3 = new Web3("https://mainnet.infura.io/v3/146c5ff4a83a4a62b8eb4bbc93e07974");
            var fullNameNode = ensUtil.GetNameHash(ensName);
            var ensRegistryService = new ENSRegistryService(web3, contract);
            var resolverAddress = await ensRegistryService.ResolverQueryAsync(
                new ResolverFunction() { Node = fullNameNode.HexToByteArray() });
            var newRes = "0x4976fb03C32e5B8cfe2b6cCB31c09Ba78EBaBa41";
            var resolverService = new PublicResolverService(web3, resolverAddress);
            var migratedResolverService = new PublicResolverService(web3, newRes);
            var address = await resolverService.AddrQueryAsync(fullNameNode.HexToByteArray());
            var migratedAddress = await migratedResolverService.AddrQueryAsync(fullNameNode.HexToByteArray());
            if (migratedAddress == "0x0000000000000000000000000000000000000000")
                return address;
            else
                return migratedAddress;
        }
    }
}