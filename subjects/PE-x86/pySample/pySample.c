// pySample.c
// Generated by decompiling D:\dev\uxmal\reko\master\subjects\PE-x86\pySample\pySample.dll
// using Decompiler version 0.5.5.0.

#include "pySample.h"

PyObject * py_unused(PyObject * self, PyObject * args)
{
	int32 eax_9 = PyArg_ParseTuple(args, ":unused", tArg00);
	if (eax_9 != 0x00)
	{
		word32 * eax_17 = _Py_NoneStruct;
		*eax_17 = *eax_17 + 0x01;
		return _Py_NoneStruct;
	}
	else
		return eax_9;
}

void initpySample()
{
	Py_InitModule4("pySample", &globals->t10003010, null, null, 1007);
	return;
}

word32 fn100011E9(word32 dwArg08)
{
	word32 eax_209;
	if (dwArg08 == 0x00)
	{
		if (globals->dw10003070 <= 0x00)
		{
			eax_209 = 0x00;
			return eax_209;
		}
		globals->dw10003070 = globals->dw10003070 - 0x01;
	}
	word32 ecx_34 = *_adjust_fdiv;
	globals->dw100033A4 = ecx_34;
	if (dwArg08 == 0x01)
	{
		Eq_71 edi_79 = fs->ptr0018->t0004;
		while (true)
		{
			*(fp - 0x1C) = 268448684;
			Eq_71 eax_89 = InterlockedCompareExchange(&globals->t100033AC, edi_79, 0x00);
			if (eax_89 == 0x00)
				break;
			if (eax_89 == edi_79)
				goto l10001258;
			*(fp - 0x14) = 1000;
			Sleep(0x00);
			*(fp - 0x14) = 0x00;
		}
l10001258:
		ptr32 esp_112;
		LONG ebp_111;
		ui32 ebx_110;
		ui32 esi_109;
		ui32 edi_108;
		word32 eax_96 = globals->dw100033A8;
		*(fp - 0x14) = 0x02;
		if (eax_96 != 0x00)
		{
			*(fp - 0x14) = 0x1F;
			word32 esp_171;
			word32 eax_172;
			byte SZO_174;
			byte C_175;
			byte SCZO_176;
			byte Z_177;
			word32 ecx_178;
			Eq_236 * fs_182;
			_amsg_exit();
			esp_112 = &fp->tFFFFFFE8;
		}
		else
		{
			*(fp - 0x14) = 0x100020A8;
			globals->dw100033A8 = 0x01;
			word32 esp_189;
			word32 eax_190;
			word32 ebp_191;
			byte SZO_192;
			byte C_193;
			byte SCZO_194;
			byte Z_195;
			word32 ecx_196;
			word32 ebx_197;
			word32 esi_198;
			word32 edi_199;
			Eq_218 * fs_200;
			_initterm_e();
			if (eax_190 != 0x00)
			{
				eax_209 = 0x00;
				return eax_209;
			}
			fp->tFFFFFFE8.u0 = 0x1000209C;
			fp->ptrFFFFFFE4 = &globals->ptr10002098;
			word32 esp_216;
			word32 eax_217;
			byte SZO_219;
			byte C_220;
			byte SCZO_221;
			byte Z_222;
			word32 ecx_223;
			Eq_451 * fs_227;
			_initterm();
			globals->dw100033A8 = 0x02;
			esp_112 = &fp->ptrFFFFFFE4;
		}
		LONG * esp_117 = esp_112 + 0x04;
		if (ebp_111 == 0x00)
		{
			*(esp_117 - 0x04) = (int32) ebp_111;
			*(esp_117 - 0x08) = (int32) 268448684;
			InterlockedExchange(*(esp_117 - 0x08), *(esp_117 - 0x04));
		}
		if (globals->ptr100033B8 != null)
		{
			Eq_374 * esp_136 = esp_117 - (LONG *) 0x04;
			Mem137[esp_136 + 0x00:word32] = 0x100033B8;
			 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * edi_138;
			word32 eax_139 = fn10001742(ebx_110, esi_109, edi_108, out edi_138);
			if (eax_139 != 0x00)
			{
				Mem146[esp_136 + 0x00:word32] = Mem137[esp_136 + 0x1C:word32];
				Mem148[esp_136 - 0x04 + 0x00:word32] = edi_138;
				Mem150[esp_136 - 0x08 + 0x00:word32] = Mem148[esp_136 + 0x14:word32];
				word32 esp_151;
				word32 eax_152;
				word32 ebp_153;
				byte SZO_154;
				byte C_155;
				byte SCZO_156;
				byte Z_157;
				word32 ecx_158;
				word32 ebx_159;
				word32 esi_160;
				word32 edi_161;
				Eq_423 * fs_162;
				globals->ptr100033B8();
			}
		}
		globals->dw10003070 = globals->dw10003070 + 0x01;
	}
	else if (dwArg08 == 0x00)
	{
		while (true)
		{
			*(fp - 0x14) = 0x00;
			*(fp - 0x18) = 0x01;
			*(fp - 0x1C) = 268448684;
			if (InterlockedCompareExchange(&globals->t100033AC, 0x01, 1000) == 0x00)
				break;
			*(fp - 0x14) = 1000;
			Sleep(0x00);
		}
		word32 eax_261 = globals->dw100033A8;
		if (eax_261 != 0x02)
		{
			*(fp - 0x14) = 0x1F;
			word32 esp_266;
			word32 eax_267;
			word32 ebp_268;
			byte SZO_269;
			byte C_270;
			byte SCZO_271;
			byte Z_272;
			word32 ecx_273;
			word32 ebx_274;
			word32 esi_275;
			word32 edi_276;
			Eq_195 * fs_277;
			_amsg_exit();
		}
		else
		{
			*(fp - 0x14) = (Eq_71 *) globals->t100033B4;
			word32 esp_284;
			word32 eax_285;
			word32 ebp_286;
			byte SZO_287;
			byte C_288;
			byte SCZO_289;
			byte Z_290;
			word32 ecx_291;
			word32 ebx_292;
			word32 esi_293;
			word32 edi_294;
			Eq_170 * fs_295;
			_decode_pointer();
			ptr32 esp_302 = &fp->tFFFFFFE8;
			if (eax_285 != 0x00)
			{
				fp->tFFFFFFE8 = globals->t100033B0;
				word32 esp_323;
				code * * eax_324;
				word32 ebp_325;
				byte SZO_326;
				byte C_327;
				byte SCZO_328;
				byte Z_329;
				word32 ecx_330;
				word32 esi_332;
				word32 edi_333;
				Eq_303 * fs_334;
				code * * ebx_331;
				_decode_pointer();
				code * * edi_338 = eax_324;
				while (true)
				{
					edi_338 = edi_338 - 0x04;
					if (edi_338 < ebx_331)
						break;
					code * eax_371 = *edi_338;
					if (eax_371 != null)
					{
						word32 esp_375;
						word32 eax_376;
						word32 ebp_377;
						byte SZO_378;
						byte C_379;
						byte SCZO_380;
						byte Z_381;
						word32 ecx_382;
						word32 esi_384;
						Eq_435 * fs_386;
						eax_371();
					}
				}
				fp->ptrFFFFFFE4 = ebx_331;
				free(&globals->t100033AC);
				word32 esp_356;
				Eq_71 eax_357;
				word32 ebp_358;
				byte SZO_359;
				byte C_360;
				byte SCZO_361;
				byte Z_362;
				word32 ecx_363;
				word32 ebx_364;
				word32 esi_365;
				word32 edi_366;
				Eq_360 * fs_367;
				_encoded_null();
				globals->t100033B0 = eax_357;
				globals->t100033B4 = eax_357;
				esp_302 = &fp->ptrFFFFFFE4;
			}
			LONG * esp_314 = esp_302 - 0x04;
			*esp_314 = (int32) 0x00;
			*(esp_314 - 0x04) = (int32) 268448684;
			globals->dw100033A8 = 0x00;
			InterlockedExchange(*(esp_314 - 0x04), *esp_314);
		}
	}
	eax_209 = 0x01;
	return eax_209;
}

void fn10001388(ui32 ecx, ui32 edx, ui32 ebx, ui32 esi, ui32 edi)
{
	Eq_465 * ebp_10 = fn100017E8(ebx, esi, edi, dwLoc0C, 0x100021E8, 0x10);
	ui32 ebx_158 = ebp_10->dw0008;
	*(ebp_10 - 0x1C) = 0x01;
	*(ebp_10 - 0x04) = 0x00;
	globals->dw10003008 = edx;
	*(ebp_10 - 0x04) = 0x01;
	ptr32 esp_175 = fp - 0x08;
	ui32 edi_12 = ecx;
	ui32 esi_14 = edx;
	if (edx == 0x00 && globals->dw10003070 == 0x00)
	{
		*(ebp_10 - 0x1C) = 0x00;
		goto l1000147A;
	}
	if (edx == 0x01 || edx == 0x02)
	{
		code * eax_165 = globals->ptr100020CC;
		if (eax_165 != null)
		{
			*(fp - 0x0C) = ecx;
			*(fp - 0x10) = edx;
			*(fp - 0x14) = ebx_158;
			word32 ecx_202;
			word32 edx_204;
			ui32 eax_207;
			byte SZO_208;
			byte C_209;
			byte SCZO_210;
			byte Z_211;
			eax_165();
			*(ebp_10 - 0x1C) = eax_207;
		}
		if (*(ebp_10 - 0x1C) == 0x00)
		{
l1000147A:
			*(ebp_10 - 0x04) = *(ebp_10 - 0x04) & 0x00;
			*(ebp_10 - 0x04) = ~0x01;
			fn10001493();
			fn1000182D(ebp_10, 0x10, dwArg00, dwArg04, dwArg08, dwArg0C);
			return;
		}
		ui32 * esp_182 = esp_175 - 0x04;
		*esp_182 = edi_12;
		*(esp_182 - 0x04) = esi_14;
		*(esp_182 - 0x08) = ebx_158;
		ui32 eax_188 = fn100011E9(dwArg04);
		*(ebp_10 - 0x1C) = eax_188;
		esp_175 = esp_182 + 0x01;
		if (eax_188 == 0x00)
			goto l1000147A;
	}
	ui32 * esp_56 = esp_175 - 0x04;
	*esp_56 = edi_12;
	*(esp_56 - 0x04) = esi_14;
	*(esp_56 - 0x08) = ebx_158;
	ui32 eax_62 = fn100017C6(0x10, dwArg04);
	*(ebp_10 - 0x1C) = eax_62;
	word32 esp_142 = esp_56 + 0x01;
	if (esi_14 == 0x01 && eax_62 == 0x00)
	{
		*esp_56 = edi_12;
		*(esp_56 - 0x04) = eax_62;
		*(esp_56 - 0x08) = ebx_158;
		fn100017C6(0x10, dwArg04);
		*esp_56 = edi_12;
		*(esp_56 - 0x04) = 0x00;
		*(esp_56 - 0x08) = ebx_158;
		fn100011E9(dwArg04);
		esp_142 = esp_56 + 0x01;
		code * eax_143 = globals->ptr100020CC;
		if (eax_143 != null)
		{
			*esp_56 = edi_12;
			*(esp_56 - 0x04) = 0x00;
			*(esp_56 - 0x08) = ebx_158;
			word32 ecx_155;
			word32 edx_157;
			word32 eax_160;
			byte SZO_161;
			byte C_162;
			byte SCZO_163;
			byte Z_164;
			eax_143();
		}
	}
	if (esi_14 == 0x00 || esi_14 == 0x03)
	{
		ui32 * esp_80 = esp_142 - 0x04;
		*esp_80 = edi_12;
		*(esp_80 - 0x04) = esi_14;
		*(esp_80 - 0x08) = ebx_158;
		ui32 eax_86 = fn100011E9(dwArg04);
		if (eax_86 == 0x00)
			*(ebp_10 - 0x1C) = *(ebp_10 - 0x1C) & eax_86;
		if (*(ebp_10 - 0x1C) != 0x00)
		{
			code * eax_95 = globals->ptr100020CC;
			if (eax_95 != null)
			{
				*esp_80 = edi_12;
				*(esp_80 - 0x04) = esi_14;
				*(esp_80 - 0x08) = ebx_158;
				word32 esp_105;
				word32 edi_106;
				word32 ecx_107;
				word32 esi_108;
				word32 edx_109;
				word32 ebx_110;
				ui32 eax_112;
				byte SZO_113;
				byte C_114;
				byte SCZO_115;
				byte Z_116;
				eax_95();
				*(ebp_10 - 0x1C) = eax_112;
			}
		}
	}
	goto l1000147A;
}

void fn10001493()
{
	globals->dw10003008 = ~0x00;
	return;
}

void fn1000149E(ui32 ebx, ui32 esi, ui32 edi, word32 dwArg00, word32 dwArg08, word32 dwArg0C)
{
	if (dwArg08 == 0x01)
		fn10001864();
	fn10001388(dwArg0C, dwArg08, ebx, esi, edi);
	return;
}

word32 fn100016D0(word32 dwArg04)
{
	if (dwArg04->w0000 == 23117)
	{
		Eq_826 * eax_21 = dwArg04 + dwArg04->dw003C / 0x0040;
		if (eax_21->dw0000 == 0x4550)
			return (word32) (eax_21->w0018 == 0x010B);
	}
	return 0x00;
}

Eq_843 * fn10001700(word32 dwArg04, word32 dwArg08)
{
	Eq_846 * ecx_6 = dwArg04 + dwArg04->dw003C / 0x0040;
	uint32 esi_14 = (word32) ecx_6->w0006;
	uint32 edx_15 = 0x00;
	Eq_843 * eax_22 = (ecx_6 + ((word32) ecx_6->w0014 + 0x18) / 22)->w0006 + 0x03;
	if (true)
		do
		{
			uint32 ecx_49 = eax_22->dw0000;
			if (dwArg08 >= ecx_49 && dwArg08 < eax_22->dw0008 + ecx_49)
				return eax_22;
			edx_15 = edx_15 + 0x01;
			eax_22 = eax_22 + 0x01;
		} while (edx_15 < esi_14);
	eax_22 = null;
	return eax_22;
}

ui32 fn10001742(ui32 ebx, ui32 esi, ui32 edi,  * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ediOut)
{
	ui32 eax_31;
	Eq_465 * ebp_10 = fn100017E8(ebx, esi, edi, dwLoc0C, 0x10002230, 0x08);
	*(ebp_10 - 0x04) = *(ebp_10 - 0x04) & 0x00;
	*(fp - 0x0C) = 0x10000000;
	if (fn100016D0(dwArg00) != 0x00)
	{
		*(fp - 0x0C) = (Eq_907 *) (ebp_10->dw0008 - 0x10000000);
		*(fp - 0x10) = 0x10000000;
		Eq_943 * eax_54 = fn10001700(dwArg00, dwArg04);
		if (eax_54 != null)
		{
			eax_31 = ~(eax_54->dw0024 >> 0x1F) & 0x01;
			*(ebp_10 - 0x04) = ~0x01;
l100017A8:
			word32 edi_37;
			*ediOut = fn1000182D(ebp_10, 0x08, dwArg00, dwArg04, dwArg08, dwArg0C);
			return eax_31;
		}
	}
	*(ebp_10 - 0x04) = ~0x01;
	eax_31 = 0x00;
	goto l100017A8;
}

word32 fn100017C6(word32 dwArg00, word32 dwArg08)
{
	if (dwArg08 == 0x01 && globals->ptr100020CC == null)
		DisableThreadLibraryCalls(dwArg00);
	return 0x01;
}

ptr32 fn100017E8(ui32 ebx, ui32 esi, ui32 edi, word32 dwArg00, word32 dwArg04, word32 dwArg08)
{
	ui32 * esp_13 = fp - 0x08 - dwArg08;
	*(esp_13 - 0x04) = ebx;
	*(esp_13 - 0x08) = esi;
	*(esp_13 - 0x0C) = edi;
	*(esp_13 - 0x10) = globals->dw10003000 ^ fp + 0x08;
	*(esp_13 - 0x14) = dwArg00;
	fs->ptr0000 = fp - 0x08;
	return fp + 0x08;
}

word32 fn1000182D(Eq_465 * ebp, word32 dwArg00, word32 dwArg04, word32 dwArg08, word32 dwArg0C, word32 dwArg10)
{
	fs->dw0000 = *(ebp - 0x10);
	ebp->dw0000 = dwArg00;
	return dwArg08;
}

void fn10001864()
{
	ui32 eax_8 = globals->dw10003000;
	if (eax_8 != 0xBB40E64E && (eax_8 & 0xFFFF0000) != 0x00)
		globals->dw10003004 = ~eax_8;
	else
	{
		GetSystemTimeAsFileTime(fp - 0x0C);
		ui32 esi_58 = dwLoc08 & 0x00 ^ dwLoc0C & 0x00 ^ GetCurrentProcessId() ^ GetCurrentThreadId() ^ GetTickCount();
		QueryPerformanceCounter(fp - 0x14);
		ui32 esi_68 = esi_58 ^ (dwLoc10 ^ dwLoc14);
		if (esi_68 == 0xBB40E64E)
			esi_68 = ~0x44BF19B0;
		else if ((esi_68 & 0xFFFF0000) == 0x00)
			esi_68 = esi_68 | esi_68 << 0x10;
		globals->dw10003000 = esi_68;
		globals->dw10003004 = ~esi_68;
	}
	return;
}

