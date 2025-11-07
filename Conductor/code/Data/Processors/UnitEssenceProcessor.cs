using Conductor.Data.Registers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.Character;
using TrainworksReloaded.Base.Extensions;
using TrainworksReloaded.Core.Impl;
using TrainworksReloaded.Core.Interfaces;

namespace Conductor.Data.Processors
{
    internal class UnitEssenceProcessor
    {
        private IRegister<CharacterData> characterRegister;
        private IRegister<CardUpgradeData> upgradeRegister;
        private UnitEssenceRegistry unitEssenceRegistry;
        private PluginAtlas atlas;

        public UnitEssenceProcessor(IRegister<CharacterData> characterRegister, IRegister<CardUpgradeData> upgradeRegister, UnitEssenceRegistry unitEssenceRegistry, PluginAtlas pluginAtlas)
        {
            this.characterRegister = characterRegister;
            this.upgradeRegister = upgradeRegister;
            this.unitEssenceRegistry = unitEssenceRegistry;
            this.atlas = pluginAtlas;
        }

        public void Run()
        {
            foreach (var config in atlas.PluginDefinitions)
            {
                var key = config.Key;
                var pluginConfig = config.Value.Configuration;
                foreach (var child in pluginConfig.GetSection("essences").GetChildren())
                {
                    var reference = child.GetSection("character").ParseReference();
                    var upgradeReference = child.GetSection("upgrade").ParseReference();
                    if (reference == null || upgradeReference == null)
                    {
                        Plugin.Logger.LogError($"Missing required properties character, upgrade for {child.Path}");
                        continue;
                    }
                    
                    if (!characterRegister.TryLookupName(reference.ToId(key, TemplateConstants.Character), out var character, out var _, reference.context))
                    {
                        continue;
                    }
                    if (!upgradeRegister.TryLookupName(upgradeReference.ToId(key, TemplateConstants.Upgrade), out var upgrade, out var _, upgradeReference.context))
                    {
                        continue;
                    }

                    // If the owner of the character adds the essence prefer that.
                    unitEssenceRegistry.Register(character!, upgrade!, reference.mod_reference == key);
                }
            }
        }
    }
}
