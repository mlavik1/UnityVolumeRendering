/*--------------------------------------------------------------------------*\
EDIT: This is a modified version
Original source: https://github.com/DannyRuijters/CubicInterpolationCUDA/blob/master/examples/glCubicRayCast/tricubic.shader

Copyright (c) 2008-2009, Danny Ruijters. All rights reserved.
http://www.dannyruijters.nl/cubicinterpolation/
This file is part of CUDA Cubic B-Spline Interpolation (CI).

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
*  Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
*  Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
*  Neither the name of the copyright holders nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are
those of the authors and should not be interpreted as representing official
policies, either expressed or implied.

When using this code in a scientific project, please cite one or all of the
following papers:
*  Daniel Ruijters and Philippe Th�venaz,
   GPU Prefilter for Accurate Cubic B-Spline Interpolation, 
   The Computer Journal, vol. 55, no. 1, pp. 15-20, January 2012.
   http://dannyruijters.nl/docs/cudaPrefilter3.pdf
*  Daniel Ruijters, Bart M. ter Haar Romeny, and Paul Suetens,
   Efficient GPU-Based Texture Interpolation using Uniform B-Splines,
   Journal of Graphics Tools, vol. 13, no. 4, pp. 61-69, 2008.
\*--------------------------------------------------------------------------*/

//! Tricubic interpolated texture lookup, using unnormalized coordinates.
//! Fast implementation, using 8 trilinear lookups.
//! @param tex  3D texture
//! @param texCoord  normalized 3D texture coordinate
//! @param texSize  size of the texture
float4 interpolateTricubicFast(sampler3D tex, float3 texCoord, float3 texSize)
{
	// shift the coordinate from [0,1] to [-0.5, texSize-0.5]
	float3 coord_grid = texCoord * texSize - 0.5;
	float3 index = floor(coord_grid);
	float3 fraction = coord_grid - index;
	float3 one_frac = 1.0 - fraction;

	float3 w0 = 1.0/6.0 * one_frac*one_frac*one_frac;
	float3 w1 = 2.0/3.0 - 0.5 * fraction*fraction*(2.0-fraction);
	float3 w2 = 2.0/3.0 - 0.5 * one_frac*one_frac*(2.0-one_frac);
	float3 w3 = 1.0/6.0 * fraction*fraction*fraction;

	float3 g0 = w0 + w1;
	float3 g1 = w2 + w3;
	float3 mult = 1.0 / texSize;
	float3 h0 = mult * ((w1 / g0) - 0.5 + index);  //h0 = w1/g0 - 1, move from [-0.5, texSize-0.5] to [0,1]
	float3 h1 = mult * ((w3 / g1) + 1.5 + index);  //h1 = w3/g1 + 1, move from [-0.5, texSize-0.5] to [0,1]

	// fetch the eight linear interpolations
	// weighting and fetching is interleaved for performance and stability reasons
	float4 tex000 = tex3Dlod(tex, float4(h0, 0.0));
	float4 tex100 = tex3Dlod(tex, float4(h1.x, h0.y, h0.z, 0.0));
	tex000 = lerp(tex100, tex000, g0.x);  //weigh along the x-direction
	float4 tex010 = tex3Dlod(tex, float4(h0.x, h1.y, h0.z, 0.0));
	float4 tex110 = tex3Dlod(tex, float4(h1.x, h1.y, h0.z, 0.0));
	tex010 = lerp(tex110, tex010, g0.x);  //weigh along the x-direction
	tex000 = lerp(tex010, tex000, g0.y);  //weigh along the y-direction
	float4 tex001 = tex3Dlod(tex, float4(h0.x, h0.y, h1.z, 0.0));
	float4 tex101 = tex3Dlod(tex, float4(h1.x, h0.y, h1.z, 0.0));
	tex001 = lerp(tex101, tex001, g0.x);  //weigh along the x-direction
	float4 tex011 = tex3Dlod(tex, float4(h0.x, h1.y, h1.z, 0.0));
	float4 tex111 = tex3Dlod(tex, float4(h1, 0.0));
	tex011 = lerp(tex111, tex011, g0.x);  //weigh along the x-direction
	tex001 = lerp(tex011, tex001, g0.y);  //weigh along the y-direction

	return lerp(tex001, tex000, g0.z);  //weigh along the z-direction
}
