﻿using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public sealed class MoveCameraAlongWaypointPathAction : MapScriptAction
    {
        private readonly WaypointPath _waypointPath;
        private readonly Vector3 _direction;
        private readonly TimeSpan _duration;
        private readonly float _shutter;

        private CameraAnimation _animation;

        public MoveCameraAlongWaypointPathAction(ScriptAction action, SceneSettings sceneSettings)
        {
            _waypointPath = sceneSettings.WaypointPaths[action.Arguments[0].StringValue];

            _direction = _waypointPath.End.Position - _waypointPath.Start.Position;

            _duration = TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value);

            // TODO: What is this?
            _shutter = action.Arguments[2].FloatValue.Value;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            if (_animation == null)
            {
                _animation = context.Scene.CameraController.StartAnimation(
                    _waypointPath.Start.Position,
                    _waypointPath.End.Position,
                    context.UpdateTime.TotalGameTime,
                    _duration);
            }

            return _animation.Finished
                ? ScriptExecutionResult.Finished
                : ScriptExecutionResult.NotFinished;
        }

        public override void Reset()
        {
            _animation = null;
        }
    }
}
