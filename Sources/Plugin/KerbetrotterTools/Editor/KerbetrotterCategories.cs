/*
 * Copyright (C) 2018 Nils277 (https://github.com/Nils277)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using KSP.Localization;

namespace KerbetrotterTools
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class KerbatrotterCategories : MonoBehaviour
    {
        //create the icons
        private Texture2D[] icon_filter;

        //The icons for the categories
        private Texture2D icon_filter_pods = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_pods", false);
        private Texture2D icon_filter_engine = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_engine", false);
        private Texture2D icon_filter_fuel = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_fueltank", false);
        private Texture2D icon_filter_payload = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_payload", false);
        private Texture2D icon_filter_construction = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_construction", false);
        private Texture2D icon_filter_coupling = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_coupling", false);
        private Texture2D icon_filter_electrical = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_electrical", false);
        private Texture2D icon_filter_ground = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_ground", false);
        private Texture2D icon_filter_utility = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_utility", false);
        private Texture2D icon_filter_science = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_science", false);
        private Texture2D icon_filter_thermal = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_thermal", false);
        private Texture2D icon_filter_aero = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_aero", false);
        private Texture2D icon_filter_control = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_control", false);
        private Texture2D icon_filter_communication = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_communication", false);
        private Texture2D icon_filter_cargo = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Icons/filter_cargo", false);
        
        //Sets whether there should be a filter
        internal bool filter = true;

        //Is set to false when an icon could not be loaded
        private bool isValid = true;

        //A dictionary storing all the categories of the parts
        private Dictionary<string, PartCategories>[] partCategories;

        //stores wheter the Community Category Kit is available
        private bool CCKavailable = false;

        //The name of the function filter
        private string filterName = "#autoLOC_453547";

        /// <summary>
        /// When the class awakes it inits all the filters it found for the KerbatrotterTools
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(this);

            //search for Community Category Kit
            int numAssemblies = AssemblyLoader.loadedAssemblies.Count;
            for (int i = 0; i < numAssemblies; i++)
            {
                if (AssemblyLoader.loadedAssemblies[i].name.Equals("CCK"))
                {
                    CCKavailable = true;
                    break;
                }
            }

            //if the configuration is null
            if (KerbetrotterConfiguration.Instance() == null)
            {
                Debug.LogError("[KerbetrotterTools:Catagories] Configuration Instance is null");
                return;
            }

            //get the filterSetings for the kerbetrotter tools
            KerbetrotterFilterSettings[] filterSettings = KerbetrotterConfiguration.Instance().FilterSettings;

            int numFilter = filterSettings.Length;
            partCategories = new Dictionary<string, PartCategories>[numFilter];

            for (int i = 0; i < numFilter; i++)
            {
                //add all the parts that should be in the list
                List<AvailablePart> all_parts = new List<AvailablePart>();

                all_parts.AddRange(PartLoader.Instance.loadedParts.FindAll(ap => ap.name.StartsWith(filterSettings[i].IncludeFilter)));

                //remove the parts that are excluded from the filter
                if (!string.IsNullOrEmpty(filterSettings[i].ExcludeFilter))
                {
                    all_parts.RemoveAll(ap => ap.name.StartsWith(filterSettings[i].ExcludeFilter));
                }
                //save all the categories from the parts of this mod
                int numParts = all_parts.Count;
                partCategories[i] = new Dictionary<string, PartCategories>(numParts);
                for (int k = 0; k < numParts; k++)
                {
                    partCategories[i].Add(all_parts[k].name, all_parts[k].category);
                }
            }

            icon_filter = new Texture2D[numFilter];

            //load the icons for the categories
            try
            {
                for (int i = 0; i < numFilter; i++)
                {
                    icon_filter[i] = GameDatabase.Instance.GetTexture(filterSettings[i].FilterIcon, false);
                    if (icon_filter[i] == null)
                    {
                        icon_filter[i] = new Texture2D(32, 32, TextureFormat.ARGB32, false);
                        Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_icon for: " + filterSettings[i].ModName);
                    }
                }

                if (icon_filter_pods == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_pods");
                    isValid = false;
                }
                if (icon_filter_aero == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_aero");
                    isValid = false;
                }
                if (icon_filter_control == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_control");
                    isValid = false;
                }
                if (icon_filter_communication == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_communication");
                    isValid = false;
                }
                if (icon_filter_fuel == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_fueltank");
                    isValid = false;
                }
                if (icon_filter_electrical == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_electrical");
                    isValid = false;
                }
                if (icon_filter_thermal == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_thermal");
                    isValid = false;
                }
                if (icon_filter_science == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_science");
                    isValid = false;
                }
                if (icon_filter_engine == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_engine");
                    isValid = false;
                }
                if (icon_filter_ground == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_ground");
                    isValid = false;
                }
                if (icon_filter_coupling == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_coupling");
                    isValid = false;
                }
                if (icon_filter_payload == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_payload");
                    isValid = false;
                }
                if (icon_filter_construction == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_construction");
                    isValid = false;
                }
                if (icon_filter_utility == null)
                {
                    Debug.LogError("[KerbetrotterTools:Catagory] Awake: loading filter_utility");
                    isValid = false;
                }
            }
            catch (Exception e)
            {
                Debug.Log("[KerbetrotterTools:Catagory] EXC Awake: cannot load images" + e.Message);
                Debug.LogException(e);
                isValid = false;
            }

            //Add the Kerbetrotterfilter to the list of filters
            GameEvents.onGUIEditorToolbarReady.Add(KerbetrotterFilter);
        }

        /// <summary>
        /// Removes all listeners from the GameEvents when Class is destroyed
        /// </summary>
        protected void OnDestroy()
        {
            GameEvents.onGUIEditorToolbarReady.Remove(KerbetrotterFilter);
        }

        /// <summary>
        /// Filters parts by their names
        /// </summary>
        /// <param name="part">the part which has to be filtered</param>
        /// <returns></returns>
        private bool filterPart(AvailablePart part, int index)
        {
            KerbetrotterFilterSettings filterSettings = KerbetrotterConfiguration.Instance().FilterSettings[index];
            return part.name.StartsWith(filterSettings.IncludeFilter) && (string.IsNullOrEmpty(filterSettings.ExcludeFilter) || !part.name.StartsWith(filterSettings.ExcludeFilter));
        }

        /**
         * Filter the parts by their manufacturer
         * 
         * @param[in] part : the part to test
         * @param[in] category : the category of the part
         * 
         * @return[bool] true when categories match, else false
         */
        private bool filterCategories(AvailablePart part, PartCategories category, int index)
        {
            if (index >= KerbetrotterConfiguration.Instance().FilterSettings.Length)
            {
                Debug.LogError("[KerbetrotterTools:Catagory] filterCategories: invalid index for category filter: " + index);
                return false;
            }

            KerbetrotterFilterSettings filterSettings = KerbetrotterConfiguration.Instance().FilterSettings[index];
            //return false when the part is not included by the filter
            if (!part.name.StartsWith(filterSettings.IncludeFilter) || (!string.IsNullOrEmpty(filterSettings.ExcludeFilter) && part.name.StartsWith(filterSettings.ExcludeFilter)))
            {
                return false;
            }
            return partCategories[index][part.name] == category;
        }

        private bool filterCategoriesMulti(AvailablePart part, PartCategories[] categories, int index)
        {
            if (index >= KerbetrotterConfiguration.Instance().FilterSettings.Length)
            {
                Debug.LogError("[KerbetrotterTools:Catagory] filterCategoriesMulti: invalid index for category filter: " + index);
                return false;
            }

            KerbetrotterFilterSettings filterSettings = KerbetrotterConfiguration.Instance().FilterSettings[index];
            //return false when the part is not included by the filter
            if (!part.name.StartsWith(filterSettings.IncludeFilter) || (!string.IsNullOrEmpty(filterSettings.ExcludeFilter) && part.name.StartsWith(filterSettings.ExcludeFilter)))
            {
                return false;
            }
            for (int i = 0; i < categories.Length; i++)
            {
                if (partCategories[index][part.name] == categories[i])
                {
                    return true;
                }
            }

            return false;
        }

        /**
         * The function to add the modules of this mod to a separate category 
         */
        private void KerbetrotterFilter()
        {
            if (!isValid)
            {
                Debug.LogError("[KerbetrotterTools:Catagory] KerbetrotterFilter: invalid");
                return;
            }

            //if the configuration is null
            if (KerbetrotterConfiguration.Instance() == null)
            {
                Debug.Log("[KerbetrotterTools:Catagory] KerbetrotterFilter: Configuration Instance is null");
                return;
            }

            //get the filterSetings for the kerbetrotter tools
            KerbetrotterFilterSettings[] filterSettings = KerbetrotterConfiguration.Instance().FilterSettings;

            //icons for KSP's own category
            RUI.Icons.Selectable.Icon ic_pods = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_pods", icon_filter_pods, icon_filter_pods, true);
            RUI.Icons.Selectable.Icon ic_aero = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_aero", icon_filter_pods, icon_filter_pods, true);
            RUI.Icons.Selectable.Icon ic_control = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_control", icon_filter_pods, icon_filter_pods, true);
            RUI.Icons.Selectable.Icon ic_communication = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_comm", icon_filter_pods, icon_filter_pods, true);
            RUI.Icons.Selectable.Icon ic_fuels = new RUI.Icons.Selectable.Icon("Kerbetrotter__filter_fuel", icon_filter_fuel, icon_filter_fuel, true);
            RUI.Icons.Selectable.Icon ic_engine = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_engine", icon_filter_engine, icon_filter_engine, true);
            RUI.Icons.Selectable.Icon ic_structural = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_structural", icon_filter_construction, icon_filter_construction, true);
            RUI.Icons.Selectable.Icon ic_payload = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_payload", icon_filter_payload, icon_filter_payload, true);
            RUI.Icons.Selectable.Icon ic_utility = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_utility", icon_filter_utility, icon_filter_utility, true);
            RUI.Icons.Selectable.Icon ic_coupling = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_coupling", icon_filter_coupling, icon_filter_coupling, true);
            RUI.Icons.Selectable.Icon ic_ground = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_ground", icon_filter_ground, icon_filter_ground, true);
            RUI.Icons.Selectable.Icon ic_thermal = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_thermal", icon_filter_thermal, icon_filter_thermal, true);
            RUI.Icons.Selectable.Icon ic_electrical = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_electrical", icon_filter_electrical, icon_filter_electrical, true);
            RUI.Icons.Selectable.Icon ic_science = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_fuel", icon_filter_science, icon_filter_science, true);
            RUI.Icons.Selectable.Icon ic_cargo = new RUI.Icons.Selectable.Icon("Kerbetrotter_filter_cargo", icon_filter_cargo, icon_filter_cargo, true);
            //RUI.Icons.Selectable.Icon ic_lifeSupport = new RUI.Icons.Selectable.Icon("Kerbetrotter_icon_life_support", icon_filter_lifesupport, icon_filter_lifesupport, true);


            int numFilter = filterSettings.Length;
            for (int i = 0; i < numFilter; i++)
            {
                //-----------------own category-----------------

                if (filterSettings[i].ShowModFilter)
                {
                    List<PartCategories> availableCategories = new List<PartCategories>(15);
                    availableCategories.Add(PartCategories.Aero);
                    availableCategories.Add(PartCategories.Communication);
                    availableCategories.Add(PartCategories.Control);
                    availableCategories.Add(PartCategories.Coupling);
                    availableCategories.Add(PartCategories.Electrical);
                    availableCategories.Add(PartCategories.Engine);
                    availableCategories.Add(PartCategories.FuelTank);
                    availableCategories.Add(PartCategories.Ground);
                    availableCategories.Add(PartCategories.Payload);
                    availableCategories.Add(PartCategories.Pods);
                    availableCategories.Add(PartCategories.Propulsion);
                    availableCategories.Add(PartCategories.Science);
                    availableCategories.Add(PartCategories.Structural);
                    availableCategories.Add(PartCategories.Thermal);
                    availableCategories.Add(PartCategories.Utility);
                    availableCategories.Add(PartCategories.Cargo);

                    List<PartCategories> usedCategories = new List<PartCategories>();

                    int numParts = partCategories[i].Count;
                    foreach (KeyValuePair<string, PartCategories> entry in partCategories[i])
                    {
                        if (availableCategories.Contains(entry.Value))
                        {
                            usedCategories.Add(entry.Value);
                            availableCategories.Remove(entry.Value);
                        }
                    }

                    if (usedCategories.Count > 0)
                    {
                        //create the icon for the filter
                        RUI.Icons.Selectable.Icon filterIcon = new RUI.Icons.Selectable.Icon(filterSettings[i].ModName + "_icon_mod", icon_filter[i], icon_filter[i], true);

                        //add the mod to the categories to the categories
                        Color color = filterSettings[i].Color;
                        PartCategorizer.Category modFilter =PartCategorizer.AddCustomFilter(filterSettings[i].ModName, filterSettings[i].ModName, filterIcon, new Color(color.r, color.g, color.b));

                        int index = i;

                        //add subcategories to the category that was just added
                        if (usedCategories.Contains(PartCategories.Pods))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Pods", Localizer.GetStringByTag("#autoLOC_453549"),  ic_pods, p => filterCategories(p, PartCategories.Pods, index));
                        }

                        if (usedCategories.Contains(PartCategories.FuelTank))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Fuel Tank", Localizer.GetStringByTag("#autoLOC_453552"), ic_fuels, p => filterCategories(p, PartCategories.FuelTank, index));
                        }

                        if (usedCategories.Contains(PartCategories.Engine) || usedCategories.Contains(PartCategories.Propulsion))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Engines", Localizer.GetStringByTag("#autoLOC_453555"), ic_engine, p => filterCategoriesMulti(p, new PartCategories[] {PartCategories.Propulsion, PartCategories.Engine}, index));
                        }

                        if (usedCategories.Contains(PartCategories.Control))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Control", Localizer.GetStringByTag("#autoLOC_453558"), ic_control, p => filterCategories(p, PartCategories.Control, index));
                        }

                        if (usedCategories.Contains(PartCategories.Structural))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Structural", Localizer.GetStringByTag("#autoLOC_453561"), ic_structural, p => filterCategories(p, PartCategories.Structural, index));
                        }

                        if (usedCategories.Contains(PartCategories.Coupling))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Coupling", Localizer.GetStringByTag("#autoLOC_453564"), ic_coupling, p => filterCategories(p, PartCategories.Coupling, index));
                        }

                        if (usedCategories.Contains(PartCategories.Payload))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Payload", Localizer.GetStringByTag("#autoLOC_453567"), ic_payload, p => filterCategories(p, PartCategories.Payload, index));
                        }

                        if (usedCategories.Contains(PartCategories.Aero))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Aerodynamics", Localizer.GetStringByTag("#autoLOC_453570"), ic_aero, p => filterCategories(p, PartCategories.Aero, index));
                        }

                        if (usedCategories.Contains(PartCategories.Ground))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Ground", Localizer.GetStringByTag("#autoLOC_453573"), ic_ground, p => filterCategories(p, PartCategories.Ground, index));
                        }

                        if (usedCategories.Contains(PartCategories.Thermal))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Thermal", Localizer.GetStringByTag("#autoLOC_453576"), ic_thermal, p => filterCategories(p, PartCategories.Thermal, index));
                        }

                        if (usedCategories.Contains(PartCategories.Electrical))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Electrical", Localizer.GetStringByTag("#autoLOC_453579"), ic_electrical, p => filterCategories(p, PartCategories.Electrical, index));
                        }

                        if (usedCategories.Contains(PartCategories.Communication))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Communication", Localizer.GetStringByTag("#autoLOC_453582"), ic_communication, p => filterCategories(p, PartCategories.Communication, index));
                        }

                        if (usedCategories.Contains(PartCategories.Science))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Science", Localizer.GetStringByTag("#autoLOC_453585"), ic_science, p => filterCategories(p, PartCategories.Science, index));
                        }

                        if (usedCategories.Contains(PartCategories.Utility))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Utility", Localizer.GetStringByTag("#autoLOC_453588"), ic_utility, p => (filterCategories(p, PartCategories.Utility, index)));
                        }

                        if (usedCategories.Contains(PartCategories.Cargo))
                        {
                            PartCategorizer.AddCustomSubcategoryFilter(modFilter, "Cargo", Localizer.GetStringByTag("##autoLOC_8320001"), ic_cargo, p => (filterCategories(p, PartCategories.Cargo, index)));
                        }
                    }
                }
                //-----------------end own category-----------------

                //------------subcategory in function filter---------
                if (filterSettings [i].ShowFunctionFilter && (!CCKavailable || !filterSettings[i].DisableForCCK)) 
                {
                    RUI.Icons.Selectable.Icon filterIconSurfaceStructures = new RUI.Icons.Selectable.Icon("Kerbetrotter_function filter", icon_filter[i], icon_filter[i], true);

                    if (filterIconSurfaceStructures == null)
                    {
                        Debug.LogError("[KerbetrotterTools:Catagory] KerbetrotterFilter: FilterIcon cannot be loaded for: " + filterSettings[i].ModName);
                        return;
                    }

                    //Find the function filter
                    PartCategorizer.Category functionFilter = PartCategorizer.Instance.filters.Find(f => f.button.categorydisplayName == filterName);

                    int index = i;

                    //Add a new subcategory to the function filter
                    PartCategorizer.AddCustomSubcategoryFilter(functionFilter, filterSettings[i].ModName, filterSettings[i].ModName, filterIconSurfaceStructures, p => filterPart(p, index));

                    //Remove the parts from all other categories
                    if (filterSettings[i].ShowInOneFilterOnly)
                    {
                        List<AvailablePart> parts = PartLoader.Instance.loadedParts.FindAll(ap => filterPart(ap, index));

                        for (int j = 0; j < parts.Count; j++)
                        {
                            parts[j].category = PartCategories.none;
                        }
                    }
                }
                //hide the parts from other functions when CCK is installed
                else if (CCKavailable && filterSettings[i].ShowInOneFilterOnly)
                {
                    int index = i;

                    //Remove the parts from all other categories
                    if (filterSettings[i].ShowInOneFilterOnly)
                    {
                        List<AvailablePart> parts = PartLoader.Instance.loadedParts.FindAll(ap => filterPart(ap, index));

                        for (int j = 0; j < parts.Count; j++)
                        {
                            parts[j].category = PartCategories.none;
                        }
                    }
                }
                //---------end subcategory in function filter-------
            }
        }
    }
}