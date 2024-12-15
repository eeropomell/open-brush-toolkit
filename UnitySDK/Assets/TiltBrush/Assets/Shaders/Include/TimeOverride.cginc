// Copyright 2017 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#ifdef SHADER_SCRIPTING_ON

  uniform float4 _TimeOverrideValue = float4(0,0,0,0);
  uniform half _TimeBlend = 0.0;
  uniform half _TimeSpeed = 1.0;

  float4 GetTime() {
    return lerp(_Time * _TimeSpeed, _TimeOverrideValue, _TimeBlend);
  }

#else

  float4 GetTime() {
    return _Time;
  }
#endif

