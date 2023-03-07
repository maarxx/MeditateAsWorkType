using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MeditateAsWorkType
{
    [StaticConstructorOnStartup]
    public class DiminishingGrassGizmo : Gizmo
    {
        public DiminishingGrassGizmo(DiminishingGrassComp connection)
        {
            this.connection = connection;
            Order = -100f;
        }

        private DiminishingGrassComp connection;

        private float selectedStrengthTarget = -1f;

        private bool draggingBar;

        private const float Width = 212f;

        private static readonly Texture2D StrengthTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Orange);

        private static readonly Texture2D StrengthHighlightTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.LightOrange);

        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

        private static readonly Texture2D StrengthTargetTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.DarkOrange);

        private float ExtraHeight = Text.LineHeight * 1.5f;

        private float DesiredConnectionStrength
        {
            get
            {
                if (!draggingBar)
                {
                    return connection.diminishingGrassThreshold;
                }
                return selectedStrengthTarget;
            }
        }

        private float OverrideHeight => 75f + ExtraHeight;

        public override float GetWidth(float maxWidth)
        {
            return 212f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            if (!ModsConfig.IdeologyActive)
            {
                return new GizmoResult(GizmoState.Clear);
            }
            Rect rect = new Rect(topLeft.x, topLeft.y - ExtraHeight, GetWidth(maxWidth), OverrideHeight);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            float curY = rect3.yMin;
            Text.Anchor = TextAnchor.UpperCenter;
            Text.Font = GameFont.Small;
            Widgets.Label(rect3.x, ref curY, rect3.width, "ConnectionStrength".Translate());
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            //Widgets.Label(rect3.x, ref curY, rect3.width, "DesiredConnectionStrength".Translate() + ": " + DesiredConnectionStrength.ToStringPercent());
            //Widgets.Label(rect3.x, ref curY, rect3.width, "PruningHoursToMaintain".Translate() + ": " + connection.PruningHoursToMaintain(DesiredConnectionStrength).ToString("F1"));
            Widgets.Label(rect3.x, ref curY, rect3.width, "DesiredConnectionStrength".Translate() + ": ");
            Widgets.Label(rect3.x, ref curY, rect3.width, "PruningHoursToMaintain".Translate() + ": ");
            Text.Font = GameFont.Small;
            if (Mouse.IsOver(rect2) && !draggingBar)
            {
                Widgets.DrawHighlight(rect2);
                TooltipHandler.TipRegion(rect2, () => GetTip(), 9493937);
            }
            DrawBar(rect2, curY);
            return new GizmoResult(GizmoState.Clear);
        }

        private string GetTip()
        {
            //string text = "DesiredConnectionStrengthDesc".Translate(connection.parent.Named("TREE"), connection.ConnectedPawn.Named("CONNECTEDPAWN"), connection.ConnectionStrengthLossPerDay.ToStringPercent().Named("FALL")).Resolve();
            //string text2 = connection.AffectingBuildingsDescription("CurrentlyAffectedBy");
            //if (!text2.NullOrEmpty())
            //{
            //    text = text + "\n\n" + text2;
            //}
            return "FOOBAR";
        }

        private void DrawThreshold(Rect rect, float percent, float strValue)
        {
            Rect rect2 = default(Rect);
            rect2.x = rect.x + 3f + (rect.width - 8f) * percent;
            rect2.y = rect.y + rect.height - 9f;
            rect2.width = 2f;
            rect2.height = 6f;
            Rect position = rect2;
            if (strValue < percent)
            {
                GUI.DrawTexture(position, BaseContent.GreyTex);
            }
            else
            {
                GUI.DrawTexture(position, BaseContent.BlackTex);
            }
        }

        private void DrawStrengthTarget(Rect rect, float percent)
        {
            float num = Mathf.Round((rect.width - 8f) * percent);
            GUI.DrawTexture(new Rect(rect.x + 3f + num, rect.y, 2f, rect.height), StrengthTargetTex);
            float num2 = Widgets.AdjustCoordToUIScalingFloor(rect.x + 2f + num);
            float xMax = Widgets.AdjustCoordToUIScalingCeil(num2 + 4f);
            Rect rect2 = default(Rect);
            rect2.y = rect.y - 3f;
            rect2.height = 5f;
            rect2.xMin = num2;
            rect2.xMax = xMax;
            Rect rect3 = rect2;
            GUI.DrawTexture(rect3, StrengthTargetTex);
            Rect position = rect3;
            position.y = rect.yMax - 2f;
            GUI.DrawTexture(position, StrengthTargetTex);
        }

        private void DrawBar(Rect inRect, float curY)
        {
            Rect rect = inRect;
            rect.xMin += 10f;
            rect.xMax -= 10f;
            rect.yMax = inRect.yMax - 4f;
            rect.yMin = curY + 10f;
            bool flag = Mouse.IsOver(rect);
            float connectionStrength = connection.diminishingGrassThreshold;
            Widgets.FillableBar(rect, connectionStrength, flag ? StrengthHighlightTex : StrengthTex, EmptyBarTex, doBorder: true);
            //foreach (CurvePoint point in connection.Props.maxDryadsPerConnectionStrengthCurve.Points)
            //{
            //    if (point.x > 0f)
            //    {
            //        DrawThreshold(rect, point.x, connectionStrength);
            //    }
            //}
            float num = Mathf.Clamp(Mathf.Round((Event.current.mousePosition.x - (rect.x + 3f)) / (rect.width - 8f) * 20f) / 20f, 0f, 1f);
            Event current2 = Event.current;
            if (current2.type == EventType.MouseDown && current2.button == 0 && flag)
            {
                selectedStrengthTarget = num;
                draggingBar = true;
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
                current2.Use();
            }
            if (current2.type == EventType.MouseDrag && current2.button == 0 && draggingBar && flag)
            {
                if (Mathf.Abs(num - selectedStrengthTarget) > float.Epsilon)
                {
                    SoundDefOf.DragSlider.PlayOneShotOnCamera();
                }
                selectedStrengthTarget = num;
                current2.Use();
            }
            if (current2.type == EventType.MouseUp && current2.button == 0 && draggingBar)
            {
                if (selectedStrengthTarget >= 0f)
                {
                    connection.diminishingGrassThreshold = selectedStrengthTarget;
                }
                selectedStrengthTarget = -1f;
                draggingBar = false;
                current2.Use();
            }
            DrawStrengthTarget(rect, DesiredConnectionStrength);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, connection.diminishingGrassThreshold.ToStringPercent());
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }

}
