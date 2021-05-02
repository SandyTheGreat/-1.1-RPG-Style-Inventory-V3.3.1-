﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using System.Linq;

namespace Sandy_Detailed_RPG_Inventory
{
    class Sandy_RPG_Settings : ModSettings
    {
        public static float rpgTabHeight = 500f;
        public static float rpgTabWidth = 706f;
        public static bool displayAllSlots = false;
        public static bool displayTempOnTheSameLine = false;
        public static Dictionary<ThingDef, bool> simplifiedView = null;
        public static Sandy_RPG_Settings instance { get { return LoadedModManager.GetMod<Sandy_Detailed_RPG_Inventory>().GetSettings<Sandy_RPG_Settings>(); }  }
        public static bool apparelHealthbar = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rpgTabHeight, "rpgTabHeight", 500f);
            Scribe_Values.Look(ref rpgTabWidth, "rpgTabWidth", 706f);
            Scribe_Values.Look(ref displayAllSlots, "displayAllSlots", false);
            Scribe_Values.Look(ref displayTempOnTheSameLine, "displayTempOnTheSameLine", false);
            Scribe_Values.Look(ref apparelHealthbar, "apparelHealthbar", false);
            //
            if (simplifiedView == null)
                return;
            //
            List<KeyValuePair<ThingDef,bool>> l = simplifiedView.ToList();
            for (var i = 0; i < l.Count; i ++)
            {
                bool b = l[i].Value;
                Scribe_Values.Look(ref b, l[i].Key.defName + "_simplifiedView", false);
                simplifiedView[l[i].Key] = b;
            }
        }


        public static void FillSimplifiedViewDict()
        {
            if (simplifiedView != null || instance == null)
                return;
            //
            simplifiedView = new Dictionary<ThingDef, bool>();
            IEnumerable<ThingDef> l = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.thingClass == typeof(Pawn) || x.thingClass.IsSubclassOf(typeof(Pawn)));
            foreach (var i in l)
            {
                Sandy_RPG_Settings.simplifiedView[i] = false;
            }
            var inst = instance;
            LoadedModManager.ReadModSettings<Sandy_RPG_Settings>(inst.Mod.Content.FolderName, inst.Mod.GetType().Name);
        }
    }

    class Sandy_Detailed_RPG_Inventory : Mod
    {
        Sandy_RPG_Settings settings;
        string tabHeight;
        string tabWidth;

        public Sandy_Detailed_RPG_Inventory(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<Sandy_RPG_Settings>();
        }

        public override string SettingsCategory()
        {
            return "RPG_Style_Inventory_Title".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            inRect.width /= 2;
            listingStandard.Begin(inRect);
            listingStandard.Label("RPG_Inventory_Width".Translate());
            listingStandard.TextFieldNumeric(ref Sandy_RPG_Settings.rpgTabWidth, ref tabWidth);
            string s;
            if (Sandy_Detailed_RPG_GearTab.minRecommendedWidth == Sandy_RPG_Settings.rpgTabWidth)
                s = "RPG_AutoFitWidth_Wide_Button_Label".Translate();
            else if (Sandy_Detailed_RPG_GearTab.maxRecommendedWidth == Sandy_RPG_Settings.rpgTabWidth)
                s = "RPG_AutoFitWidth_Tight_Button_Label".Translate();
            else
                s = "RPG_AutoFitWidth_Button_Label".Translate();
            if (listingStandard.ButtonText(s))
            {
                DoFit(Sandy_RPG_Settings.displayAllSlots);
            }
            listingStandard.Gap();
            listingStandard.Label("RPG_Inventory_Height".Translate());
            listingStandard.TextFieldNumeric(ref Sandy_RPG_Settings.rpgTabHeight, ref tabHeight);
            //
            if (Sandy_Utility.CustomCheckboxLabeled(listingStandard, "RPG_Display_All_Slots_Label".Translate(), ref Sandy_RPG_Settings.displayAllSlots, "RPG_Display_All_Slots_Note".Translate()))
            {
                DoFit(Sandy_RPG_Settings.displayAllSlots, true);
            }
            listingStandard.CheckboxLabeled("RPG_Dispaly_Temp_On_The_Same_Line_Label".Translate(), ref Sandy_RPG_Settings.displayTempOnTheSameLine, "RPG_Dispaly_Temp_On_The_Same_Line_Note".Translate());
            listingStandard.CheckboxLabeled("RPG_Display_Apparel_Healthbar_Label".Translate(), ref Sandy_RPG_Settings.apparelHealthbar, "RPG_Display_Apparel_Healthbar_Note".Translate());

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        protected void DoFit(bool displayAllSlots, bool reset = false)
        {
            Sandy_Detailed_RPG_GearTab.MakePreps(displayAllSlots, reset);
            float minWidth = Sandy_Detailed_RPG_GearTab.minRecommendedWidth;
            float maxWidth = Sandy_Detailed_RPG_GearTab.maxRecommendedWidth;

            if(!reset)
                if (Sandy_RPG_Settings.rpgTabWidth == Sandy_Detailed_RPG_GearTab.minRecommendedWidth) //ability to switch between recommended sizes
                {
                    tabWidth = maxWidth.ToString();
                    return;
                }
                else if (Sandy_RPG_Settings.rpgTabWidth == Sandy_Detailed_RPG_GearTab.maxRecommendedWidth)
                {
                    tabWidth = minWidth.ToString();
                    return;
                }

            if (Sandy_RPG_Settings.rpgTabWidth < minWidth) // setting minimum size (pawn model on the bottom)
                {
                    tabWidth = minWidth.ToString();
                }
                else if (Sandy_RPG_Settings.rpgTabWidth > minWidth) //stats on the side
                {
                    if (Sandy_RPG_Settings.rpgTabWidth < maxWidth)
                    {
                        tabWidth = minWidth.ToString();
                    }
                    else
                    {
                        tabWidth = maxWidth.ToString();
                    }
                }
        }
    }

    [StaticConstructorOnStartup]
    public static class Sandy_Utility
    {
        public static readonly Texture2D texBar = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));
        public static readonly Texture2D texYellow = SolidColorMaterials.NewSolidColorTexture(Color.yellow);
        public static readonly Texture2D texRed = SolidColorMaterials.NewSolidColorTexture(Color.red);
        public static readonly Texture2D texBlack = SolidColorMaterials.NewSolidColorTexture(Color.black);
        public static readonly Texture2D texBG = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true);
        public static readonly Texture2D texTainted = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Tainted_Icon", true);
        public static readonly Texture2D texForced = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Forced_Icon", true);
        public static readonly Texture2D texMass = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_MassCarried_Icon", true);
        public static readonly Texture2D texMinTemp = ContentFinder<Texture2D>.Get("UI/Icons/Min_Temperature", true);
        public static readonly Texture2D texMaxTemp = ContentFinder<Texture2D>.Get("UI/Icons/Max_Temperature", true);
        public static readonly Texture2D texButtonDrop = ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true);
        public static readonly Texture2D texButtonIngest = ContentFinder<Texture2D>.Get("UI/Buttons/Ingest", true);
        public static readonly Texture2D texArmorSharp = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorSharp_Icon", true);
        public static readonly Texture2D texArmorBlunt = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorBlunt_Icon", true);
        public static readonly Texture2D texArmorHeat = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorHeat_Icon", true);
        public static readonly Texture2D texTattered = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Tattered");
        public static readonly Texture2D texNotTattered = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Not_Tattered");

        public static readonly string frameFolder = "AltFrames";
        public static readonly Texture2D texLegendary = ContentFinder<Texture2D>.Get("UI/" + frameFolder + "/RPG_Legendary", true);
        public static readonly Texture2D texMasterwork = ContentFinder<Texture2D>.Get("UI/" + frameFolder + "/RPG_Masterwork", true);
        public static readonly Texture2D texExcellent = ContentFinder<Texture2D>.Get("UI/" + frameFolder + "/RPG_Excellent", true);
        public static readonly Texture2D texGood = ContentFinder<Texture2D>.Get("UI/" + frameFolder + "/RPG_Good", true);
        public static readonly Texture2D texNormal = ContentFinder<Texture2D>.Get("UI/" + frameFolder + "/RPG_Normal", true);
        public static readonly Texture2D texPoor = ContentFinder<Texture2D>.Get("UI/" + frameFolder + "/RPG_Poor", true);
        public static readonly Texture2D texAwful = ContentFinder<Texture2D>.Get("UI/" + frameFolder + "/RPG_Awful", true);

        public static bool CustomCheckboxLabeled(Listing listing, string label, ref bool checkOn, string tooltip = null)
        {
            bool result = false;
            float lineHeight = Text.LineHeight;
            bool val = checkOn;
            Func<bool, bool> check = delegate (bool pressed) { result = pressed; if (pressed) val = !val; return val; };
            Rect rect = listing.GetRect(lineHeight);
            CustomCheckboxLabeled(rect, label, check, tooltip);
            checkOn = val;
            listing.Gap(listing.verticalSpacing);
            return result;
        }

        public static void CustomCheckboxLabeled(Rect rect, string label, Func<bool, bool> func, string tooltip = null)
        {
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect)) Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, tooltip);
            }
            //
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rect, label);
            bool val;
            if (Widgets.ButtonInvisible(rect, true))
            {
                val = func(true);
                if (val) SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
                else SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
            }
            else val = func(false);
            //
            Color color = GUI.color;
            Texture2D image;
            if (val) image = Widgets.CheckboxOnTex;
            else image = Widgets.CheckboxOffTex;
            GUI.DrawTexture(new Rect(rect.x + rect.width - 24f, rect.y, 24f, 24f), image);
            Text.Anchor = anchor;
        }
    }
}