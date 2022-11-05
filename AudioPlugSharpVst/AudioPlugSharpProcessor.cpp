#include "public.sdk/source/vst/vstaudioprocessoralgo.h"

#include "public.sdk/source/vst/vstaudioeffect.h"
#include "pluginterfaces/vst/ivstevents.h"
#include "pluginterfaces/vst/ivstparameterchanges.h"
#include "pluginterfaces/base/ibstream.h"
#include "base/source/fstreamer.h"

#include "base/source/fobject.h"


#include <sstream>

#include "AudioPlugSharpProcessor.h"
#include "AudioPlugSharpController.h"
#include "AudioPlugSharpFactory.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace System::Reflection;

using namespace AudioPlugSharp;

FUID AudioPlugSharpProcessor::AudioPlugSharpProcessorUID;

AudioPlugSharpProcessor::AudioPlugSharpProcessor(IAudioPluginProcessor^ managed): managedProcessor(managed)
{
	setControllerClass(FUID(AudioPlugSharpController::AudioPlugSharpControllerUID));
}

AudioPlugSharpProcessor::~AudioPlugSharpProcessor(void)
{
}

FUnknown* AudioPlugSharpProcessor::createInstance(void* factory)
{
	Logger::Log("Create processor instance");

	auto apsFactory = static_cast<AudioPlugSharpFactory*>(factory);

	auto assembly = Assembly::GetExecutingAssembly()->Location;

	Logger::Log("createInstance: " + assembly);

	auto managed = apsFactory->Load<IAudioPluginProcessor>();
	IAudioProcessor* processor = new AudioPlugSharpProcessor(managed);

	return processor;
}

tresult PLUGIN_API AudioPlugSharpProcessor::initialize(FUnknown* context)
{
	tresult result = AudioEffect::initialize(context);

	if (result != kResultOk)
	{
		return result;
	}

	try
	{
		audioPlugHost = gcnew AudioPlugSharpHost();

		//plugin->Host = audioPlugHost;

		managedProcessor->Initialize();
	}
	catch (Exception^ ex)
	{
		Logger::Log("Unable to initialize managed processor: " + ex->ToString());
	}

	// Add audio inputs
	for each (auto port in managedProcessor->InputPorts)
	{
		TChar* portName = (TChar*)(void*)Marshal::StringToHGlobalUni(port->Name);

		addAudioInput(portName, port->ChannelConfiguration == EAudioChannelConfiguration::Mono ? SpeakerArr::kMono : SpeakerArr::kStereo);

		Marshal::FreeHGlobal((IntPtr)portName);
	}

	// Add audio outputs
	for each (auto port in managedProcessor->OutputPorts)
	{
		TChar* portName = (TChar*)(void*)Marshal::StringToHGlobalUni(port->Name);

		addAudioOutput(portName, port->ChannelConfiguration == EAudioChannelConfiguration::Mono ? SpeakerArr::kMono : SpeakerArr::kStereo);

		Marshal::FreeHGlobal((IntPtr)portName);
	}

	// Set up an event intput
	addEventInput(STR16("Event In"), 1);

	return kResultOk;
}

tresult PLUGIN_API AudioPlugSharpProcessor::terminate()
{
	return AudioEffect::terminate();
}

tresult PLUGIN_API AudioPlugSharpProcessor::setActive(TBool state)
{
	if (state)
	{
		managedProcessor->Start();
	}
	else
	{
		managedProcessor->Stop();
	}

	return AudioEffect::setActive(state);
}

tresult PLUGIN_API AudioPlugSharpProcessor::setState(IBStream* state)
{
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

		managedProcessor->RestoreState(byteArray);
	}

	return kResultOk;
}

tresult PLUGIN_API AudioPlugSharpProcessor::getState(IBStream* state)
{
	Logger::Log("Save State");

	auto data = managedProcessor->SaveState();

	if (data != nullptr)
	{
		unsigned char* dataChars = new unsigned char[data->Length];

		Marshal::Copy(data, 0, (IntPtr)dataChars, data->Length);

		state->write(dataChars, data->Length, 0);

		delete[] dataChars;
	}

	return kResultOk;
}

tresult PLUGIN_API AudioPlugSharpProcessor::canProcessSampleSize(int32 symbolicSampleSize)
{
	if (symbolicSampleSize == kSample32)
	{
		if ((managedProcessor->SampleFormatsSupported & EAudioBitsPerSample::Bits32) == EAudioBitsPerSample::Bits32)
		{
			return kResultTrue;
		}
	}

	if (symbolicSampleSize == kSample64)
	{
		if ((managedProcessor->SampleFormatsSupported & EAudioBitsPerSample::Bits64) == EAudioBitsPerSample::Bits64)
		{
			return kResultTrue;
		}
	}

	return kResultFalse;
}

tresult PLUGIN_API AudioPlugSharpProcessor::setBusArrangements(SpeakerArrangement* inputs, int32 numIns, SpeakerArrangement* outputs, int32 numOuts)
{
	// We should be ok with any arrangement

	return kResultOk;
}

tresult PLUGIN_API AudioPlugSharpProcessor::notify(Vst::IMessage* message)
{
	Logger::Log("Got message from controller");

	return kResultTrue;
}


tresult PLUGIN_API AudioPlugSharpProcessor::setupProcessing(ProcessSetup& newSetup)
{
	Logger::Log("Setup Processing. " + ((newSetup.symbolicSampleSize == kSample32) ? "32bit" : "64bit"));

	tresult result = AudioEffect::setupProcessing(newSetup);

	if (result != kResultOk)
	{
		Logger::Log("Setup processing failed");

		return result;
	}

	audioPlugHost->SampleRate = newSetup.sampleRate;
	audioPlugHost->BitsPerSample = (newSetup.symbolicSampleSize == kSample32) ? EAudioBitsPerSample::Bits32 : EAudioBitsPerSample::Bits64;
	audioPlugHost->MaxAudioBufferSize = newSetup.maxSamplesPerBlock;

	managedProcessor->InitializeProcessing();

	return kResultOk;
}

tresult PLUGIN_API AudioPlugSharpProcessor::process(ProcessData& data)
{
	IParameterChanges* paramChanges = data.inputParameterChanges;

	// Handle parameter changes
	if (paramChanges)
	{
		int32 numParamsChanged = paramChanges->getParameterCount();

		for (int32 i = 0; i < numParamsChanged; i++)
		{
			IParamValueQueue* paramQueue = paramChanges->getParameterData(i);

			if (paramQueue)
			{
				ParamValue value;
				int32 sampleOffset;
				int32 numPoints = paramQueue->getPointCount();

				ParamID paramID = paramQueue->getParameterId();

				// Only getting the last value - probably should get them all and pass them on with sample offsets...
				if (paramQueue->getPoint(numPoints - 1, sampleOffset, value) ==	kResultTrue)
				{					
					//managedProcessor->Parameters[paramID - PLUGIN_PARAMETER_USER_START]->NormalizedValue = value;
				}
			}
		}
	}

	// Handle MIDI events
	IEventList* eventList = data.inputEvents;

	if (eventList)
	{
		int32 numEvent = eventList->getEventCount();
		for (int32 i = 0; i < numEvent; i++)
		{
			Event event;
			if (eventList->getEvent(i, event) == kResultOk)
			{
				switch (event.type)
				{
					case Event::kNoteOnEvent:
					{
						managedProcessor->HandleNoteOn(event.noteOn.pitch, event.noteOn.velocity);

						break;
					}
					case Event::kNoteOffEvent:
					{
						managedProcessor->HandleNoteOff(event.noteOff.pitch, event.noteOff.velocity);

						break;
					}
					case Event::kPolyPressureEvent:
						managedProcessor->HandlePolyPressure(event.polyPressure.pitch, event.polyPressure.pressure);

						break;
				}
			}
		}
	}

	if ((data.numInputs == 0) && (data.numOutputs == 0))
	{
		// The host is just flushing events without sending audio data
	}
	else
	{
		for (int input = 0; input < managedProcessor->InputPorts->Length; input++)
		{
			managedProcessor->InputPorts[input]->SetAudioBufferPtrs((IntPtr)getChannelBuffersPointer(processSetup, data.inputs[input]),
				(data.symbolicSampleSize == kSample32) ? EAudioBitsPerSample::Bits32 : EAudioBitsPerSample::Bits64, data.numSamples);
		}

		for (int output = 0; output < managedProcessor->OutputPorts->Length; output++)
		{
			managedProcessor->OutputPorts[output]->SetAudioBufferPtrs((IntPtr)getChannelBuffersPointer(processSetup, data.outputs[output]),
				(data.symbolicSampleSize == kSample32) ? EAudioBitsPerSample::Bits32 : EAudioBitsPerSample::Bits64, data.numSamples);
		}

		managedProcessor->Process();
	}

	// Handle any output parameter changes (such as volume meter output)
	// We don't have any
	IParameterChanges* outParamChanges = data.outputParameterChanges;

	return kResultOk;
}

tresult PLUGIN_API AudioPlugSharpProcessor::queryInterface(const char* iid, void** obj)
{
	if (::Steinberg::FUnknownPrivate::iidEqual(iid, IEditController::iid))
	{
		return ::Steinberg::kResultOk;
	}

    return AudioEffect::queryInterface(iid, obj);
}
