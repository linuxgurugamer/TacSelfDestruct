/**
 * TacSelfDestruct.cs
 * 
 * Thunder Aerospace Corporation's Self Destruct for the Kerbal Space Program, by Taranis Elsu
 * 
 * (C) Copyright 2013, Taranis Elsu
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Thunder Aerospace Corporation is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace Tac
{
    [KSPModule("TAC Self Destruct")]
    public class TacSelfDestruct : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Time Delay"),
         UI_FloatRange(minValue = 1.0f, maxValue = 60.0f, stepIncrement = 1.0f, scene = UI_Scene.All)]
        public float timeDelay = 10.0f;

        [KSPField(isPersistant = true)]
        public bool canStage = true;

        [KSPField(isPersistant = true)]
        public int stagingMode = (int)StagingMode.SelfDestruct;

        [KSPField]
        public bool addPawIdent = false;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Countdown"),
         UI_Toggle(scene = UI_Scene.All, enabledText = "Visible", disabledText = "Hidden")]
        public bool showCountdown = true;

        private float countDownInitiated = 0.0f;
        private bool staged = false;
        private bool abortCountdown = false;
        private bool countDownActive = false;

        public override void OnAwake()
        {
            // this.Log("OnAwake");
        }

        public override void OnStart(PartModule.StartState state)
        {
            this.Log("OnStart");
            part.stackIcon.SetIconColor(XKCDColors.FireEngineRed);
            part.ActivatesEvenIfDisconnected = true;

            if (!canStage)
            {
                Invoke("DisableStaging", 0.5f);
            }
            else
            {
                UpdateStagingEvents();
            }

            UpdateSelfDestructEvents();
            GameEvents.onVesselChange.Add(OnVesselChange);
        }
        // OnStart isn't called in the Editor, so these need to be in Start
        void Start()
        {
            if (addPawIdent)
            {
                Events["EnableStaging"].guiName = "TacSelfDestruct: Enable Staging";
                Events["DisableStaging"].guiName = "TacSelfDestruct: Disable Staging";
            }
        }
        public override void OnInitialize()
        {
            // this.Log("OnInitialize");
        }

        public override void OnActive()
        {
            this.Log("Activating!");
            if (canStage && countDownInitiated == 0.0f)
            {
                countDownInitiated = Time.time;
                StartCoroutine(DoSelfDestruct());
                staged = true;
                UpdateSelfDestructEvents();
#if false
                switch ((StagingMode)stagingMode)
                {
                    case StagingMode.SelfDestruct:
                        ExplodeAllEvent();
                        break;
                    case StagingMode.DetonateParent:
                        DetonateParentEvent();
                        break;
                }
#endif
            }
        }

        public void OnVesselChange(Vessel newVessel)
        {
            if (newVessel == this.vessel && !canStage)
            {
                Invoke("DisableStaging", 0.5f);
            }
        }

        public override string GetInfo()
        {
            return "Default delay = " + timeDelay + " (tweakable)";
        }

        [KSPEvent(active = false, guiActive = false, guiActiveEditor = true, guiName = "Enable Staging")]
        public void EnableStaging()
        {
            part.deactivate();
            part.inverseStage = Math.Min(StageManager.LastStage, part.inverseStage);
            part.stackIcon.CreateIcon();
            StageManager.Instance.SortIcons(true);

            canStage = true;
            UpdateStagingEvents();
        }

        [KSPEvent(active = false, guiActive = false, guiActiveEditor = true, guiName = "Disable Staging")]
        public void DisableStaging()
        {
            part.stackIcon.RemoveIcon();
            StageManager.Instance.SortIcons(true);

            canStage = false;
            UpdateStagingEvents();
        }

        [KSPEvent(active = false, guiActive = true, guiActiveEditor = true, guiName = "Mode: Detonate Parent")]
        public void DetonateParentOnStaging()
        {
            stagingMode = (int)StagingMode.SelfDestruct;
            UpdateStagingModeEvents();
        }

        [KSPEvent(active = false, guiActive = true, guiActiveEditor = true, guiName = "Mode: Self Destruct")]
        public void SelfDestructOnStaging()
        {
            stagingMode = (int)StagingMode.DetonateParent;
            UpdateStagingModeEvents();
        }

        private void UpdateStagingEvents()
        {
            UpdateStagingModeEvents();
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (canStage)
                {
                    Events["EnableStaging"].active = false;
                    Events["DisableStaging"].active = true;
                }
                else
                {
                    Events["EnableStaging"].active = true;
                    Events["DisableStaging"].active = false;
                }
            }
            else
            {
                Events["EnableStaging"].active = false;
                Events["DisableStaging"].active = false;
            }
        }

        private void UpdateStagingModeEvents()
        {
            Events["DetonateParentOnStaging"].active = false;
            Events["SelfDestructOnStaging"].active = false;

            if (canStage)
            {
                switch ((StagingMode)stagingMode)
                {
                    case StagingMode.SelfDestruct:
                        Events["SelfDestructOnStaging"].active = true;
                        break;
                    case StagingMode.DetonateParent:
                        Events["DetonateParentOnStaging"].active = true;
                        break;
                }
            }
        }

        [KSPEvent(active = false, guiActive = true, guiActiveUnfocused = true, guiName = "Self Destruct!", unfocusedRange = 8.0f)]
        public void ExplodeAllEvent()
        {
            countDownInitiated = Time.time;
            Logging.Log("Tac.ExplodeAllEvent");
            StartCoroutine(DoSelfDestruct());
            UpdateSelfDestructEvents();
        }

        [KSPEvent(active = false, guiActive = true, guiActiveUnfocused = true, guiName = "Explode!", unfocusedRange = 8.0f)]
        public void ExplodeEvent()
        {
            part.explode();
        }

        [KSPEvent(active = false, guiActive = true, guiActiveUnfocused = true, guiName = "Detonate parent!", unfocusedRange = 8.0f)]
        public void DetonateParentEvent()
        {
            part.parent.explode();
            part.explode();
        }

        [KSPEvent(active = false, guiActive = true, guiActiveUnfocused = true, guiName = "Abort Self Destruct", unfocusedRange = 8.0f)]
        public void AbortSelfDestruct()
        {
            abortCountdown = true;
        }

        [KSPAction("Self Destruct!")]
        public void ExplodeAllAction(KSPActionParam param)
        {
            countDownActive = true;
            if (countDownInitiated == 0.0f)
            {
                countDownInitiated = Time.time;
                StartCoroutine(DoSelfDestruct());
                UpdateSelfDestructEvents();
            }

        }

        [KSPAction("Detonate parent!")]
        public void ExplodeParentAction(KSPActionParam param)
        {
            if (countDownInitiated == 0.0f)
            {
                DetonateParentEvent();
            }
        }

        [KSPAction("Explode!")]
        public void ExplodeAction(KSPActionParam param)
        {
            if (countDownInitiated == 0.0f)
            {
                part.explode();
            }
        }

        private void UpdateSelfDestructEvents()
        {
            UpdateStagingModeEvents();
            if (countDownInitiated == 0.0f)
            {
                // countdown has not been started
                Events["ExplodeAllEvent"].active = true;
                Events["ExplodeEvent"].active = true;
                Events["DetonateParentEvent"].active = true;
                Events["AbortSelfDestruct"].active = false;
            }
            else
            {
                // countdown has been started
                Events["ExplodeAllEvent"].active = false;
                Events["ExplodeEvent"].active = false;
                Events["DetonateParentEvent"].active = false;
                Events["AbortSelfDestruct"].active = true;
            }
        }

        private IEnumerator<WaitForSeconds> DoSelfDestruct()
        {
            ScreenMessage msg = null;
            if (showCountdown)
            {
                msg = ScreenMessages.PostScreenMessage("Self destruct sequence initiated.", 1.0f, ScreenMessageStyle.UPPER_CENTER);
            }

            while ((Time.time - countDownInitiated) < timeDelay && !abortCountdown)
            {
                float remaining = timeDelay - (Time.time - countDownInitiated);
                if (showCountdown)
                {
                    UpdateCountdownMessage(ref msg, remaining);
                    ScreenMessages.PostScreenMessage(msg.message, 1.0f, ScreenMessageStyle.UPPER_CENTER);
                }
                yield return new WaitForSeconds(1.0f);
            }

            if (!abortCountdown)
            {
                if (staged)
                {
                    if ((StagingMode)stagingMode == StagingMode.DetonateParent)
                    {
                        DetonateParentEvent();
                        yield break;
                    }
                }

                while (vessel.parts.Count > 0)
                {
                    // We do not want to blow up the root part nor the self destruct part until last.
                    // also, ignore the various LES parts, in case they are there and waiting to react to an explosion
                    Part part = vessel.parts.Find(p => p != vessel.rootPart && p != this.part && !p.children.Any() &&
                        p.partInfo.name != "pkLES.EscapeMk1v2" && p.partInfo.name != "pkLES.mk2" && p.partInfo.name != "pkLES.mk2.noBPC" &&
                        p.partInfo.name != "LaunchEscapeSystem"
                    );
                    if (part != null)
                    {
                        part.explode();
                        // Do a yield here in case something else (ie:  Bob's Panic Box) needs to react to the part exploding
                        yield return null;
                    }
                    else
                    {
                        // Explode the rest of the parts
                        while (vessel.parts.Count > 0)                           
                        {
                            vessel.parts[0].explode();
                            // Do a yield here in case something else (ie:  Bob's Panic Box) needs to react to the part exploding
                            yield return null;
                        }
                    }
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else
            {
                // reset
                if (countDownActive)
                {
                    msg.startTime = Time.time;
                    msg.duration = 5.0f;
                    msg.message = "Self destruct sequence stopped.";
                }
                countDownActive = false;
                part.deactivate();
                countDownInitiated = 0.0f;
                abortCountdown = false;
                UpdateSelfDestructEvents();
            }
        }

        private void UpdateCountdownMessage(ref ScreenMessage msg, float remaining)
        {
            Debug.Log("UpdatecountdownMessage, countdownActive: " + countDownActive.ToString());
            // If the countdown message is currently being displayed
            //if (countDownActive)
            {
                // If it is still supposed to be displayed
                if (showCountdown)
                {
                    // Update the countdown message
                    msg.message = "Self destruct sequence initiated: " + remaining.ToString("#0");
                }
                else
                {
                    // The UI must have been hidden or the user changed showCountdown?
                    msg.duration = 1.0f;
                    msg = null;
                }
            }
        }

        private enum StagingMode
        {
            SelfDestruct = 0,
            DetonateParent = 1
        }
    }
}
