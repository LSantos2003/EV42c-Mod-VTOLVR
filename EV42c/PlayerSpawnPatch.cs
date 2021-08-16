using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EV42c
{
    [HarmonyPatch(typeof(Actor), "Start")]
    class PlayerSpawnPatch
    {
        public static void Postfix(Actor __instance)
        {
            if (__instance.isPlayer && VTOLAPI.GetPlayersVehicleEnum() == VTOLVehicles.AV42C)
            {

                GameObject radarObj = GameObject.Instantiate(UnitCatalogue.GetUnitPrefab("E-4").GetComponentInChildren<Radar>().gameObject);
                radarObj.SetActive(false);
                radarObj.transform.SetParent(__instance.transform);
                radarObj.transform.localPosition = new Vector3(0, 1.8f, -7.13f);
                radarObj.transform.localEulerAngles = Vector3.zero;
                radarObj.transform.localScale = new Vector3(0.3977f, 0.3977f, 0.3977f);

                Radar radar = radarObj.GetComponent<Radar>();
                radar.myActor = __instance;
                radarObj.SetActive(true);

                __instance.transform.GetComponentInChildren<ModuleRWR>(true).myActor = __instance;

                GameObject radarScreenObj = GameObject.Instantiate(Main.radarPrefab);
                radarScreenObj.transform.SetParent(__instance.transform);
                radarScreenObj.transform.localPosition = new Vector3(0.528f, 0.98f, 0.609f);
                radarScreenObj.transform.localEulerAngles = Vector3.zero;
                radarScreenObj.transform.localScale = new Vector3(0.28386f, 0.28386f, 0.28386f);

                radarScreenObj.SetActive(false);
                RadarDisplay display = radarScreenObj.AddComponent<RadarDisplay>();
                display.radar = radar;
                display.myActor = __instance;
                radarScreenObj.SetActive(true);
            }
        }
    }
}
