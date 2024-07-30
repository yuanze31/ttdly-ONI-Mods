﻿using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Detours;
using PeterHan.PLib.Options;
using UnityEngine;

namespace PackAnything.Movable {
  public class GeyserMovable : BaseMovable {
    private static readonly IDetouredField<Studyable, bool> studied = PDetours.DetourField<Studyable, bool>("studied");
    private static readonly Tag smallVolcanoTag = new Tag("GeyserGeneric_small_volcano");
    private NeutroniumMover neutroniumMover;

    protected override void OnSpawn() {
      base.OnSpawn();
      var offset = new[] { -1, 0, 1, 2 };
      if (gameObject.PrefabID() == smallVolcanoTag &&
          !NeutroniumMover.CellIsUnobtanium(Grid.OffsetCell(originCell, -1, -1))) {
        offset = new[] { 0, 1, 2 };
      }
      neutroniumMover = new NeutroniumMover() {
        neutroniumOffsets = offset
      };
    }

    public override void StableMove(int targetCell) {
      base.StableMove(targetCell);
      var posCbc = gameObject.transform.position;
      posCbc.z -= 0.15f;
      gameObject.transform.SetPosition(posCbc);
      neutroniumMover.Move(originCell, targetCell);
    }


    public override void Move(int targetCell) {
      base.Move(targetCell);
      var cloned = Util.KInstantiate(Assets.GetPrefab(gameObject.PrefabID()));
      // 同步间歇泉研究状态
      if (gameObject.GetComponent<Studyable>().Studied)
        studied.Set(cloned.AddOrGet<Studyable>(), true);
      neutroniumMover.Move(originCell, targetCell);
      // 同步数值
      if (SingletonOptions<Options>.Instance.ToggleGeyserAttribute) ToggleGeyser(cloned);
      // 设置间歇泉的位置
      var posCbc = Grid.CellToPosCBC(targetCell, Grid.SceneLayer.BuildingBack);
      posCbc.z -= 0.15f; // 从官方代码复制，不加 z 轴设置间歇泉的位置会异常
      cloned.transform.SetPosition(posCbc);
      cloned.SetActive(true);
      gameObject.DeleteObject();
    }

    public void ToggleGeyser(GameObject cloned) {
      if (gameObject.TryGetComponent(out UserNameable userNameable))
        cloned.AddOrGet<UserNameable>().savedName = userNameable.savedName;

      if (!gameObject.TryGetComponent(out Geyser geyser)) return;
      var clonedGeyser = cloned.AddOrGet<Geyser>();
      clonedGeyser.configuration = geyser.configuration;
    }

    #region 补丁

    [HarmonyPatch(typeof(GeyserGenericConfig), nameof(GeyserGenericConfig.CreateGeyser))]
    private class GeyserGenericConfig_Patch {
      public static void Postfix(GameObject __result) {
        __result.AddOrGet<GeyserMovable>();
      }
    }

    #endregion
  }
}