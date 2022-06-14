﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Vintagestory.API.Client
{
    public class TextureDictionary : FakeDictionary<string, CompositeTexture>
    {
        internal bool alreadyBaked = false;

        public TextureDictionary() : base(2)
        {
        }

        public TextureDictionary(int initialCapacity) : base(initialCapacity)
        {
        }

        /// <summary>
        /// Called by the texture atlas manager when building up the block atlas. Has to add all of the blocks textures
        /// </summary>
        public virtual void BakeAndCollect(IAssetManager manager, ITextureLocationDictionary mainTextureDict, AssetLocation sourceCode, string sourceMessage)
        {
            if (alreadyBaked) return;

            foreach (var ct in Values)
            {
                ct.Bake(manager);

                BakedCompositeTexture[] bct = ct.Baked.BakedVariants;
                if (bct != null)
                {
                    for (int i = 0; i < bct.Length; i++)
                    {
                        if (!mainTextureDict.ContainsKey(bct[i].BakedName))
                            mainTextureDict.AddTextureLocation_Checked(new AssetLocationAndSource(bct[i].BakedName, sourceMessage, sourceCode, i + 1));
                    }
                    continue;
                }

                if (!mainTextureDict.ContainsKey(ct.Baked.BakedName))
                    mainTextureDict.AddTextureLocation_Checked(new AssetLocationAndSource(ct.Baked.BakedName, sourceMessage, sourceCode));
            }

            alreadyBaked = true;
        }
    }
}
