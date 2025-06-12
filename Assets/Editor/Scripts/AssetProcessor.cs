using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetProcessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (assetPath.Contains("_n.") || assetPath.Contains("_normal"))
        {
            TextureImporter texture_importer = (TextureImporter)assetImporter;
            texture_importer.textureType = TextureImporterType.NormalMap;
        }
        if (assetPath.Contains("_m.") || assetPath.Contains("_metallic") || assetPath.Contains("_occ") || assetPath.Contains("_ao"))
        {
            TextureImporter texture_importer  = (TextureImporter)assetImporter;
            texture_importer.sRGBTexture = false;
        }
    }

	void OnPreprocessModel ()
	{
		ModelImporter model_importer = (ModelImporter)assetImporter;
		model_importer.secondaryUVHardAngle = 70;
		model_importer.secondaryUVPackMargin = 10;
	}
}