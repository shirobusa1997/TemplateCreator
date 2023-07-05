// =========================================================================
//  __  __      _ _______ _____ _______ _    _ _____ _____ ____  
// |  \/  |    | |__   __/ ____|__   __| |  | |  __ \_   _/ __ \ 
// | \  / |    | |  | | | (___    | |  | |  | | |  | || || |  | |
// | |\/| |_   | |  | |  \___ \   | |  | |  | | |  | || || |  | |
// | |  | | |__| |  | |  ____) |  | |  | |__| | |__| || || |__| |
// |_|  |_|\____/   |_| |_____/   |_|   \____/|_____/_____\____/ 
//
// Project Name : TemplateCreator
// Created by   : Mojatto (Mojatto Studio)
//
// Copyright (c) 2023 Mojatto Studio All Rights Reserved.
// =========================================================================

//
using UnityEngine;
using UnityEditor;

// Package Namespaces
namespace MJTStudio.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public static class InspectorLibrary
    {
        /// <summary>
        /// 
        /// </summary>
        private const float defaultFooterButtonWidth = 125f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentationUrl"></param>
        /// <returns></returns>
        public static bool RenderDocumentationButton(string documentationUrl, float width = defaultFooterButtonWidth)
        {
            //
            if (string.IsNullOrEmpty(documentationUrl))
            {
                //
                return false;
            }

            var buttonContent = new GUIContent()
            {
                image = EditorGUIUtility.IconContent("_Help").image,
                text  = "Documentation",
                tooltip = documentationUrl
            };

            //
            if (GUILayout.Button(buttonContent, EditorStyles.miniButton, GUILayout.MaxWidth(width)))
            {
                Application.OpenURL(documentationUrl);
                return true;
            }

            //
            return false;
        }
    }
}