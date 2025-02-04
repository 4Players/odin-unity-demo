#import "AtmokyTrueSpatialUnityAppController.h"

extern "C" {
struct UnityAudioEffectDefinition;
extern void UnityRegisterAudioPlugin (int (*) (UnityAudioEffectDefinition***));
extern int atmoky_trueSpatial_GetAudioEffectDefinitions (UnityAudioEffectDefinition***);
}

@implementation AtmokyTrueSpatialUnityAppController

- (void)preStartUnity
{
    [super preStartUnity];
    UnityRegisterAudioPlugin (atmoky_trueSpatial_GetAudioEffectDefinitions);
}

@end

IMPL_APP_CONTROLLER_SUBCLASS (AtmokyTrueSpatialUnityAppController);
