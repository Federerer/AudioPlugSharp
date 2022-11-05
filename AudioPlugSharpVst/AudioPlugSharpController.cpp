#include "pluginterfaces/base/ibstream.h"
#include "base/source/fstreamer.h"

#include "AudioPlugSharpProcessor.h"
#include "AudioPlugSharpController.h"
#include "AudioPlugSharpFactory.h"
#include <sstream>

FUID AudioPlugSharpController::AudioPlugSharpControllerUID;

using namespace System;
using namespace System::Runtime::InteropServices;

AudioPlugSharpController::AudioPlugSharpController(IAudioPluginController^ managed): managedController(managed)
{
}

AudioPlugSharpController::~AudioPlugSharpController(void)
{
}

FUnknown* AudioPlugSharpController::createInstance(void* factory)
{
	Logger::Log("Create controller instance");

	auto apsFactory = static_cast<AudioPlugSharpFactory*>(factory);

	auto managedController = apsFactory->Load<IAudioPluginController>();
	AudioPlugSharpController* controller = new AudioPlugSharpController(managedController);

	return (IAudioProcessor*)controller;
}

tresult PLUGIN_API AudioPlugSharpController::initialize(FUnknown* context)
{
	tresult result = EditController::initialize(context);

	if (result != kResultOk)
	{
		return result;
	}

	try
	{
		managedController->Initialize();

		uint16 paramID = PLUGIN_PARAMETER_USER_START;

		for each (auto parameter in managedController->Parameters)
		{
			Logger::Log("Registering parameter: " + parameter->Name);

			TChar* paramName = (TChar*)(void*)Marshal::StringToHGlobalUni(parameter->Name);

			parameters.addParameter(paramName, nullptr, 0, parameter->GetValueNormalized(parameter->DefaultValue), ParameterInfo::kCanAutomate, paramID);

			Marshal::FreeHGlobal((IntPtr)paramName);

			paramID++;
		}
	}
	catch (Exception^ ex)
	{
		Logger::Log("Unable to initialize managed editor: " + ex->ToString());
	}

	return result;
}

tresult PLUGIN_API AudioPlugSharpController::terminate()
{
	return EditController::terminate();
}

tresult PLUGIN_API AudioPlugSharpController::setParamNormalized(ParamID tag, ParamValue value)
{
	if (tag < PLUGIN_PARAMETER_USER_START)
	{
		return kResultFalse;
	}

	managedController->Parameters[tag - PLUGIN_PARAMETER_USER_START]->NormalizedValue = value;

	return kResultOk;
}

ParamValue PLUGIN_API AudioPlugSharpController::getParamNormalized(ParamID tag)
{
	if (tag < PLUGIN_PARAMETER_USER_START)
	{
		return 0;
	}

	return managedController->Parameters[tag - PLUGIN_PARAMETER_USER_START]->NormalizedValue;
}

tresult PLUGIN_API AudioPlugSharpController::getParamStringByValue(ParamID tag, ParamValue valueNormalized, String128 string)
{
	if (tag < PLUGIN_PARAMETER_USER_START)
	{
		return kResultFalse;
	}

	TChar* paramStr = (TChar*)(void*)Marshal::StringToHGlobalUni(managedController->Parameters[tag - PLUGIN_PARAMETER_USER_START]->DisplayValue);

	strcpy16(string, paramStr);

	Marshal::FreeHGlobal((IntPtr)paramStr);

	return kResultOk;
}

tresult PLUGIN_API AudioPlugSharpController::setComponentState(IBStream* state)
{
	Logger::Log("Controller setComponentState");

	Logger::Log("Restore State");

	if (state != nullptr)
	{
		std::stringstream stringStream;

		char readBuf[1024];

		int32 numRead;

		do
		{
			state->read(readBuf, 1024, &numRead);

			stringStream.write(readBuf, numRead);
		} while (numRead == 1024);

		std::string dataString = stringStream.str();

		array<Byte>^ byteArray = gcnew array<Byte>(dataString.size());

		Marshal::Copy((IntPtr)&dataString[0], byteArray, 0, byteArray->Length);

		managedController->SetComponentState(byteArray);
	}

	return kResultOk;
}

void AudioPlugSharpController::sendIntMessage(const char* idTag, const Steinberg::int64 value)
{
	if (auto* message = allocateMessage())
	{
		const FReleaser releaser(message);

		message->setMessageID(idTag);
		message->getAttributes()->setInt(idTag, value);

		sendMessage(message);
	}
}

tresult PLUGIN_API AudioPlugSharpController::connect(IConnectionPoint* other)
{
	tresult result = EditController::connect(other);

	Logger::Log("Connect controller to processor");

	sendIntMessage("AudioPlugSharpControllerPtr", (Steinberg::int64)this);

	return result;
}


IPlugView* PLUGIN_API AudioPlugSharpController::createView(const char* name)
{
	Logger::Log("Create Editor View");

	auto view = managedController->CreateView();

	if (!view)
	{
		return nullptr;
	}

	editorView = new AudioPlugSharpEditor(this, view);

	return editorView;
}

tresult PLUGIN_API AudioPlugSharpController::getMidiControllerAssignment(int32 busIndex, int16 channel,
	CtrlNumber midiControllerNumber,
	ParamID& tag /*out*/)
{
	if (busIndex == 0)
	{
		AudioPluginParameter^ parameter = managedController->GetParameterByMidiController(midiControllerNumber);

		if (parameter != nullptr)
		{
			for (int i = 0; i < managedController->Parameters->Count; i++)
			{
				if (managedController->Parameters[i] == parameter)
				{
					tag = PLUGIN_PARAMETER_USER_START + i;

					return kResultTrue;
				}
			}
		}
	}

	return kResultFalse;
}


tresult PLUGIN_API AudioPlugSharpController::queryInterface(const char* iid, void** obj)
{
	QUERY_INTERFACE(iid, obj, IMidiMapping::iid, IMidiMapping)
		return EditController::queryInterface(iid, obj);
}


