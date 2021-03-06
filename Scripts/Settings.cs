// Copyright (C) by Upvoid Studios
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using Engine;
using Engine.Input;
using Engine.Universe;
using Engine.Rendering;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Webserver;
using Engine.Network;

using Newtonsoft.Json;

namespace UpvoidMiner
{
        public static class Settings
    {
        private static float FieldOfView = float.NaN;

                public static void InitSettingsHandlers()
        {
            Webserver.DefaultWebserver.RegisterDynamicContent(UpvoidMiner.ModDomain, "Settings", webSettings);
            Webserver.DefaultWebserver.RegisterDynamicContent(UpvoidMiner.ModDomain, "FieldOfView", fieldOfViewSettings);

            FieldOfView = (float)Scripting.GetUserSettingNumber("Graphics/Field of View", 75f);
            LocalScript.camera.HorizontalFieldOfView = FieldOfView;
                }

                [Serializable]
                class SettingsInfo
                {
                        public bool afterImage;
                        public bool bloom;
                        public bool lensFlares;
                        public bool noise;
                        public bool ssao;
                        public bool tonemapping;
                        public bool volumetricScattering;
                        public bool shadows;
                        public bool fog;
                        public bool fxaa;

            public bool fullscreen;

            public string resolution;
            public string[] supportedModes;

            public double lod;
        }

        static void webSettings(WebRequest request, WebResponse response)
        {
            // Handle 'Apply' request from the gui
            if (request.GetQuery("applySettings") != "")
                applySettings(request);
            else // If no apply action was sent, return the current settings in json format
                getSettings(response);
        }

        static void fieldOfViewSettings(WebRequest request, WebResponse response)
        {
            // Handle 'Apply' request from the gui
            if (request.GetQuery("set") != "")
            {
                FieldOfView = float.Parse(request.GetQuery("set"));
                LocalScript.camera.HorizontalFieldOfView = FieldOfView;
                Scripting.SetUserSettingNumber("Graphics/Field of View", FieldOfView);
            }
            else // If no apply action was sent, return the current settings in json format
            {
                response.AppendBody(LocalScript.camera.HorizontalFieldOfView.ToString());
            }
        }

                static void applySettings(WebRequest request)
                {
                        Scripting.SetUserSetting("Graphics/Enable Lensflares", Boolean.Parse(request.GetQuery("lensFlares")));
                        Scripting.SetUserSetting("Graphics/Enable Volumetric Scattering", Boolean.Parse(request.GetQuery("volumetricScattering")));
                        Scripting.SetUserSetting("Graphics/Enable Bloom", Boolean.Parse(request.GetQuery("bloom")));
                        Scripting.SetUserSetting("Graphics/Enable AfterImage", Boolean.Parse(request.GetQuery("afterImage")));
                        Scripting.SetUserSetting("Graphics/Enable Tonemapping", Boolean.Parse(request.GetQuery("tonemapping")));
                        Scripting.SetUserSetting("Graphics/Enable Noise", Boolean.Parse(request.GetQuery("noise")));
                        Scripting.SetUserSetting("Graphics/Enable Shadows", Boolean.Parse(request.GetQuery("shadows")));
                        Scripting.SetUserSetting("Graphics/Enable SSAO", Boolean.Parse(request.GetQuery("ssao")));
                        Scripting.SetUserSetting("Graphics/Enable Fog", Boolean.Parse(request.GetQuery("fog")));
                        Scripting.SetUserSetting("Graphics/Enable FXAA", Boolean.Parse(request.GetQuery("fxaa")));

            bool fullscreen = Boolean.Parse(request.GetQuery("fullscreen"));
            if (fullscreen)
                Scripting.SetUserSettingString("WindowManager/Fullscreen", "0");
            else
                Scripting.SetUserSettingString("WindowManager/Fullscreen", "-1");

            string resolution = request.GetQuery("resolution");
            if (resolution == "Native Resolution" || resolution == "Native+Resolution")
                resolution = "-1x-1";
            Scripting.SetUserSettingString("WindowManager/Resolution", resolution);


            // Compute both lod settings from the single GUI slider value.
            double falloff = 10;
            double minDis = 0;

            double lod = Double.Parse(request.GetQuery("lod"));
            if(lod<1 && lod >= 0)
            {
                falloff = 10 + lod * 30.0;
                minDis = 0;
            }
            else if(lod <= 2)
            {
                falloff = 40;
                minDis = (lod - 1) * 30.0;
            }

            Scripting.SetUserSettingNumber("Graphics/Lod Falloff", falloff);
            Scripting.SetUserSettingNumber("Graphics/Min Lod Range", minDis);

                }

                static void getSettings(WebResponse response)
                {
                        SettingsInfo info = new SettingsInfo();

            // Read the current graphics flags
                        info.lensFlares = Scripting.GetUserSetting("Graphics/Enable Lensflares", false);
                        info.volumetricScattering = Scripting.GetUserSetting("Graphics/Enable Volumetric Scattering", true);
                        info.bloom = Scripting.GetUserSetting("Graphics/Enable Bloom", true);
                        info.afterImage = Scripting.GetUserSetting("Graphics/Enable AfterImage", true);
                        info.tonemapping = Scripting.GetUserSetting("Graphics/Enable Tonemapping", true);
                        info.noise = Scripting.GetUserSetting("Graphics/Enable Noise", false);
                        info.shadows = Scripting.GetUserSetting("Graphics/Enable Shadows", true);
                        info.ssao = Scripting.GetUserSetting("Graphics/Enable SSAO", true);
                        info.fog = Scripting.GetUserSetting("Graphics/Enable Fog", true);
                        info.fxaa = Scripting.GetUserSetting("Graphics/Enable FXAA", true);

            // Currently, only the main screen can be set for fullscreen mode.
            info.fullscreen = Scripting.GetUserSettingString("WindowManager/Fullscreen", "-1") != "-1";

            // Read the supported video modes
            List<string> modes = Rendering.GetSupportedVideoModes();
            modes.Insert(0, "Native Resolution");

            info.resolution = Scripting.GetUserSettingString("WindowManager/Resolution", "-1x-1");

            if (info.resolution == "-1x-1")
                info.resolution = "Native Resolution";

            info.supportedModes = modes.Distinct().ToArray();

            double lod = (Scripting.GetUserSettingNumber("Graphics/Lod Falloff", 40.0)-10.0)/30.0;
            lod += Scripting.GetUserSettingNumber("Graphics/Min Lod Range", 0.0)/30.0;

            if (lod < 0)
                lod = 0;
            if (lod > 2)
                lod = 2;

            info.lod = lod;

            // Serialize to json to be read by the gui
                        StringWriter writer = new StringWriter();
                        JsonSerializer json = new JsonSerializer();
                        JsonTextWriter jsonWriter = new JsonTextWriter(writer);
                        json.Formatting = Formatting.Indented;
                        json.Serialize(jsonWriter, info);
                        response.AddHeader("Content-Type", "application/json");
                        response.AppendBody(writer.GetStringBuilder().ToString());
                }
    }
}

