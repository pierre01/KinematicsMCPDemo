using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Biosero.TeachPendant.Common;
using CommunityToolkit.Mvvm.Messaging;
using KinematicsDemo.Messages;
using KinematicsDemo.Models;
using KinematicsDemo.ViewModels;

namespace KinematicsDemo.Services;

internal class WebServerCommandParser : IWebServerCommandParser
{
    private readonly Dictionary<string, Func<RobotCommandInfo, TeachPendantViewModel, WebServerResponse>> _commands =
        new Dictionary<string, Func<RobotCommandInfo, TeachPendantViewModel, WebServerResponse>>();

    public WebServerCommandParser()
    {
        _commands = CreateCommands();
    }

    private Dictionary<string, Func<RobotCommandInfo, TeachPendantViewModel, WebServerResponse>> CreateCommands()
    {
        return new Dictionary<string, Func<RobotCommandInfo, TeachPendantViewModel, WebServerResponse>>()
        {
            { TeachPendantWebApiResources.Coordinates, CoordinatesCommand },
            { TeachPendantWebApiResources.Move, MoveCommand },
            { TeachPendantWebApiResources.Play, PlayCommand },
            { TeachPendantWebApiResources.RailPosition, RailPositionCommand },
            { TeachPendantWebApiResources.RecordedPoints, RecordedPointsCommand },
            { TeachPendantWebApiResources.RecordPoint, RecordPointCommand },
            { TeachPendantWebApiResources.StepPrecision, StepPrecisionCommand },
        };
    }

    public void ParseCommand(TeachPendantViewModel teachPendantViewModel, WebServerRequest webServerRequest)
    {
        var command = webServerRequest.RobotCommandInfo.Command;
        if (string.IsNullOrEmpty(command) ||
            !_commands.ContainsKey(command))
        {
            return;
        }

        var result = _commands[command].Invoke(webServerRequest.RobotCommandInfo, teachPendantViewModel);
        result.Context = webServerRequest.Context;
        WeakReferenceMessenger.Default.Send(new WebServerResponseMessage(result));
    }

    private WebServerResponse CoordinatesCommand(RobotCommandInfo commandInfo, TeachPendantViewModel viewModel)
    {
        var coordinate = new RobotCoordinate(double.Parse(viewModel.XString), double.Parse(viewModel.YString), double.Parse(viewModel.ZString), double.Parse(viewModel.RailPositionString));
        return new WebServerResponse(coordinate);
    }

    private  WebServerResponse MoveCommand(RobotCommandInfo commandInfo, TeachPendantViewModel viewModel)
    {
        var coordinate = commandInfo.Coordinate;

        //var taskRail = Task.Run(() => ProcessRobotCommand(viewModel.GoForward, viewModel.GoBackward, coordinate.Rail));
        //var taskX = Task.Run(() => ProcessRobotCommand(viewModel.GoEast, viewModel.GoWest, coordinate.X));
        //var taskY = Task.Run(() => ProcessRobotCommand(viewModel.GoNorth, viewModel.GoSouth, coordinate.Y));
        //var taskZ = Task.Run(() => ProcessRobotCommand(viewModel.GoUp, viewModel.GoDown, coordinate.Z));

        //Task.WaitAll(taskRail, taskX, taskY, taskZ);
        Application.Current.Dispatcher.Invoke(async () =>
        {

            if(coordinate.X > 0)
            {
                viewModel.StepPrecision = coordinate.X;
                viewModel.GoEast();
            }
            
            if(coordinate.X < 0)
            {
                viewModel.StepPrecision = Math.Abs(coordinate.X);
                viewModel.GoWest();
            }

            if(coordinate.Y > 0)
            {
                viewModel.StepPrecision = coordinate.Y;
                viewModel.GoNorth();
            }

            if(coordinate.Y < 0)
            {
                viewModel.StepPrecision = Math.Abs(coordinate.Y);
                viewModel.GoSouth();
            }

            if(coordinate.Z > 0)
            {
                viewModel.StepPrecision = coordinate.Z;
                viewModel.GoUp();
            }

            if(coordinate.Z < 0)
            {
                viewModel.StepPrecision = Math.Abs(coordinate.Z);
                viewModel.GoDown();
            }

            Thread.Sleep(100);
        });

        return new WebServerResponse();
    }

    private WebServerResponse PlayCommand(RobotCommandInfo commandInfo, TeachPendantViewModel viewModel)
    {
        Application.Current.Dispatcher.Invoke(() => viewModel.Play());
        return new WebServerResponse();
    }

    private WebServerResponse RailPositionCommand(RobotCommandInfo commandInfo, TeachPendantViewModel viewModel)
        => new WebServerResponse(viewModel.RailPosition);

    private WebServerResponse RecordedPointsCommand(RobotCommandInfo commandInfo, TeachPendantViewModel viewModel)
        => new WebServerResponse(viewModel.RecordedMetaPoints);

    private WebServerResponse RecordPointCommand(RobotCommandInfo commandInfo, TeachPendantViewModel viewModel)
    {
        Application.Current.Dispatcher.Invoke(() => viewModel.RecordPoint());
        return new WebServerResponse();
    }

    private WebServerResponse StepPrecisionCommand(RobotCommandInfo commandInfo, TeachPendantViewModel viewModel)
        => new WebServerResponse(viewModel.StepPrecision);

    private void ProcessRobotCommand(Action goPositiveAction, Action goNegativeAction, double value)
    {
        if (value == 0)
        {
            return;
        }

        Application.Current.Dispatcher.Invoke(async () =>
        {
            for (int i = 0; i < Math.Abs(value); ++i)
            {
                await Task.Delay(100);
                if (value > 0)
                {
                    goPositiveAction();
                }
                else if (value < 0)
                {
                    goNegativeAction();
                }
            }
        });
    }
}
