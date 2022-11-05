#include "AudioPlugSharpEditor.h"
#include "AudioPlugSharpProcessor.h"
#include "AudioPlugSharpController.h";
#include "AudioPlugSharpFactory.h"

using namespace AudioPlugSharp;

AudioPlugSharpEditor::AudioPlugSharpEditor(EditController* controller, IAudioPluginEditor^ managed)
	: EditorView(controller, nullptr), managedEditor(managed)
{
	double scale = managedEditor->GetDpiScale();

	rect.right = managedEditor->EditorWidth * scale;
	rect.bottom = managedEditor->EditorHeight * scale;
}

AudioPlugSharpEditor::~AudioPlugSharpEditor(void)
{
}

tresult PLUGIN_API AudioPlugSharpEditor::isPlatformTypeSupported(FIDString type)
{
	Logger::Log("IsPlatformSupported");

	if (strcmp(type, kPlatformTypeHWND) == 0)
	{
		Logger::Log("HWND supported");

		return kResultTrue;
	}

	Logger::Log("Not supported");

	return kResultFalse;
}

tresult PLUGIN_API AudioPlugSharpEditor::onSize(ViewRect* newSize)
{
	if (newSize)
		rect = *newSize;

	double scale = managedEditor->GetDpiScale();

	managedEditor->ResizeEditor(newSize->getWidth() / scale, newSize->getHeight() / scale);

	return kResultTrue;
}

void AudioPlugSharpEditor::attachedToParent()
{
	Logger::Log("Show Editor");

	managedEditor->ShowEditor((IntPtr)systemWindow);
}

void AudioPlugSharpEditor::removedFromParent()
{
	Logger::Log("Hide Editor");

	managedEditor->HideEditor();
}
