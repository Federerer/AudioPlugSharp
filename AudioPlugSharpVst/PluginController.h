#pragma once
#include "ManagedObject.h"
#include "AudioPlugSharpController.h"

using namespace System;

namespace AudioPlugSharp
{
	public ref class PluginController : public ManagedObject<AudioPlugSharpController>, public IPluginController
	{
	public:
		PluginController(AudioPlugSharpController*  instance) : ManagedObject<AudioPlugSharpController>(instance)
		{
				
		}

		virtual void AddParmameter(AudioPluginParameter^ parameter, unsigned short paramID)
		{
			GetInstance()->addParameter(parameter, paramID);
		}
	};
}

