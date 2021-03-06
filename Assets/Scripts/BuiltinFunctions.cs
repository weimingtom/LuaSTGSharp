﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using SLua;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

public static class BuiltinFunctions
{
	public static readonly Regex AttrRegex = new Regex(@"^(\w*?)([xy]?)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetTitle(IntPtr l)
	{
		return LuaDLL.luaL_error(l, "This function has not implemented.");
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int Print(IntPtr l)
	{
		var n = LuaDLL.lua_gettop(l);
		LuaDLL.lua_getglobal(l, "tostring"); // ... f
		LuaDLL.lua_pushstring(l, ""); // ... f s
		for (var i = 1; i <= n; ++i)
		{
			if (i > 1)
			{
				LuaDLL.lua_pushstring(l, "\t"); // ... f s s
				LuaDLL.lua_concat(l, 2); // ... f s
			}
			LuaDLL.lua_pushvalue(l, -2); // ... f s f
			LuaDLL.lua_pushvalue(l, i); // ... f s f arg[i]
			LuaDLL.lua_call(l, 1, 1); // ... f s ret
			LuaDLL.luaL_checktype(l, -1, LuaTypes.LUA_TSTRING);
			LuaDLL.lua_concat(l, 2); // ... f s
		}

		LuaDLL.luaL_checktype(l, -1, LuaTypes.LUA_TSTRING);
		Game.GameInstance.GameLogger.Log(LuaDLL.lua_tostring(l, -1));
		LuaDLL.lua_pop(l, 2);
			
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	[LuaFunctionAliasAs("sin")]
	public static int Sin(IntPtr l)
	{
		LuaDLL.lua_pushnumber(l, Mathf.Sin((float) LuaDLL.luaL_checknumber(l, 1) * Mathf.Deg2Rad));
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	[LuaFunctionAliasAs("cos")]
	public static int Cos(IntPtr l)
	{
		LuaDLL.lua_pushnumber(l, Mathf.Cos((float) LuaDLL.luaL_checknumber(l, 1) * Mathf.Deg2Rad));
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	[LuaFunctionAliasAs("asin")]
	public static int ASin(IntPtr l)
	{
		LuaDLL.lua_pushnumber(l, Mathf.Asin((float) LuaDLL.luaL_checknumber(l, 1)) * Mathf.Rad2Deg);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	[LuaFunctionAliasAs("acos")]
	public static int ACos(IntPtr l)
	{
		LuaDLL.lua_pushnumber(l, Mathf.Acos((float) LuaDLL.luaL_checknumber(l, 1)) * Mathf.Rad2Deg);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	[LuaFunctionAliasAs("tan")]
	public static int Tan(IntPtr l)
	{
		LuaDLL.lua_pushnumber(l, Mathf.Tan((float) LuaDLL.luaL_checknumber(l, 1) * Mathf.Deg2Rad));
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	[LuaFunctionAliasAs("atan")]
	public static int ATan(IntPtr l)
	{
		LuaDLL.lua_pushnumber(l, Mathf.Atan((float) LuaDLL.luaL_checknumber(l, 1)) * Mathf.Rad2Deg);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	[LuaFunctionAliasAs("atan2")]
	public static int ATan2(IntPtr l)
	{
		LuaDLL.lua_pushnumber(l,
			Mathf.Atan2((float) LuaDLL.luaL_checknumber(l, 1),
				(float) LuaDLL.luaL_checknumber(l, 2)) * Mathf.Rad2Deg);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int Angle(IntPtr l)
	{
		float x, y;

		if (LuaDLL.lua_gettop(l) == 2)
		{
			if (!LuaDLL.lua_istable(l, 1) || !LuaDLL.lua_istable(l, 2))
				return LuaDLL.luaL_error(l, "invalid lstg object for 'Angle'.");
			LuaDLL.lua_rawgeti(l, 1, 2);  // t(object) t(object) ??? id
			LuaDLL.lua_rawgeti(l, 2, 2);  // t(object) t(object) ??? id id

			var obj1 = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -2));
			var obj2 = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -1));

			if (obj1 == null || obj2 == null)
			{
				return LuaDLL.luaL_error(l, "invalid lstg object for 'Angle'.");
			}

			var obj1Pos = obj1.CurrentPosition;
			var obj2Pos = obj2.CurrentPosition;

			x = obj2Pos.x - obj1Pos.x;
			y = obj2Pos.y - obj1Pos.y;
		}
		else
		{
			x = (float) (LuaDLL.luaL_checknumber(l, 3) - LuaDLL.luaL_checknumber(l, 1));
			y = (float) (LuaDLL.luaL_checknumber(l, 4) - LuaDLL.luaL_checknumber(l, 2));
		}

		LuaDLL.lua_pushnumber(l, Mathf.Atan2(y, x) * Mathf.Rad2Deg);

		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int Dist(IntPtr l)
	{
		if (LuaDLL.lua_gettop(l) == 2)
		{
			if (!LuaDLL.lua_istable(l, 1) || !LuaDLL.lua_istable(l, 2))
			{
				return LuaDLL.luaL_error(l, "invalid lstg object for 'Dist'.");
			}
			LuaDLL.lua_rawgeti(l, 1, 2);
			LuaDLL.lua_rawgeti(l, 2, 2);
			var obj1 = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -2));
			var obj2 = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -1));
			if (obj1 == null || obj2 == null)
			{
				return LuaDLL.luaL_error(l, "invalid lstg object for 'Dist'.");
			}
			LuaDLL.lua_pushnumber(l, Vector2.Distance(obj1.CurrentPosition, obj2.CurrentPosition));
		}
		else
		{
			var p1 = new Vector2((float) LuaDLL.luaL_checknumber(l, 1), (float) LuaDLL.luaL_checknumber(l, 2));
			var p2 = new Vector2((float) LuaDLL.luaL_checknumber(l, 3), (float) LuaDLL.luaL_checknumber(l, 4));
			LuaDLL.lua_pushnumber(l, Vector2.Distance(p1, p2));
		}

		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int GetV(IntPtr l)
	{
		if (!LuaDLL.lua_istable(l, 1))
			return LuaDLL.luaL_error(l, "invalid lstg object for 'GetV'.");

		float v, a;
		LuaDLL.lua_rawgeti(l, 1, 2);  // t(object) ??? id
		Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -1)).GetV(out v, out a);

		LuaDLL.lua_pushnumber(l, v);
		LuaDLL.lua_pushnumber(l, a);

		return 2;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetV(IntPtr l)
	{
		if (!LuaDLL.lua_istable(l, 1))
			return LuaDLL.luaL_error(l, "invalid lstg object for 'GetV'.");
		
		float v = (float) LuaDLL.luaL_checknumber(l, 2), a = (float) LuaDLL.luaL_checknumber(l, 3);
		var rot = false;

		if (LuaDLL.lua_gettop(l) == 4)
		{
			LuaObject.checkType(l, 4, out rot);
		}

		LuaDLL.lua_rawgeti(l, 1, 2);  // t(object) ??? id
		var obj = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -1));
		obj.SetV(v, a, rot);

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetWindowed(IntPtr l)
	{
		bool value;
		if (LuaObject.checkType(l, -1, out value))
		{
			return LuaObject.error(l, "invalid argument for 'SetWindowed'");
		}
		Screen.fullScreen = value;
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int GetFPS(IntPtr l)
	{
		LuaDLL.lua_pushnumber(l, Game.GameInstance.CurrentFPS);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetFPS(IntPtr l)
	{
		int fps;
		if (!LuaObject.checkType(l, -1, out fps) || fps <= 0)
		{
			fps = 60;
		}
		Application.targetFrameRate = fps;
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetVsync(IntPtr l)
	{
		bool value;
		if (LuaObject.checkType(l, -1, out value))
		{
			QualitySettings.vSyncCount = value ? 1 : 0;
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int Snapshot(IntPtr l)
	{
		string path;
		LuaObject.checkType(l, -1, out path);
		if (path == null)
		{
			return LuaDLL.luaL_error(l, "Path is not a valid argument for 'Snapshot'");
		}

		Application.CaptureScreenshot(Path.Combine(Game.GameInstance.DataPath, path));
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int GetKeyState(IntPtr l)
	{
		LuaObject.pushValue(l, Input.GetKey((KeyCode) LuaDLL.luaL_checkinteger(l, 1)));
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int GetLastKey(IntPtr l)
	{
		LuaDLL.lua_pushinteger(l, (int) Game.GameInstance.LastKey);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int GetAxisState(IntPtr l)
	{
		string axisName;
		LuaObject.checkType(l, 1, out axisName);
		if (string.IsNullOrEmpty(axisName))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'GetAxisState'");
		}

		LuaObject.pushValue(l, Input.GetAxis(axisName));
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int LoadPack(IntPtr l)
	{
		string packName, password = null;
		LuaObject.checkType(l, 1, out packName);
		if (LuaDLL.lua_gettop(l) > 1)
		{
			LuaObject.checkType(l, 2, out password);
		}

		if (packName == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'LoadPack'");
		}

		var resourcePackStream = Game.GameInstance.ResourceManager.GetResourceStream(packName, password);
		if (resourcePackStream == null)
		{
			return LuaDLL.luaL_error(l, "pack '{0}' cannot be loaded.", packName);
		}

		Game.GameInstance.ResourceManager.AddResourceDataProvider(new ResourcePack(resourcePackStream));
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int UnloadPack(IntPtr l)
	{
		// TODO: 未实现
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int ExtractRes(IntPtr l)
	{
		string path, target;
		LuaObject.checkType(l, 1, out path);
		LuaObject.checkType(l, 2, out target);
		if (path == null || target == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'ExtractRes'");
		}

		try
		{
			using (var file = new FileStream(target, FileMode.CreateNew))
			{
				var resource = Game.GameInstance.ResourceManager.GetResourceStream(path);
				if (resource == null)
				{
					return LuaDLL.luaL_error(l, "resource '{0}' cannot be loaded", path);
				}

				resource.CopyTo(file);
			}
		}
		catch (Exception e)
		{
			return LuaDLL.luaL_error(l, "unhandled exception {0} caught.", e);
		}
		
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int DoFile(IntPtr l)
	{
		string path;
		LuaObject.checkType(l, -1, out path);
		if (string.IsNullOrEmpty(path))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'DoFile'");
		}

		Game.GameInstance.ResourceManager.FindResourceAs<ResLuaScript>(path).Execute(LuaState.get(l));
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int GetCollisionLayerId(IntPtr l)
	{
		string layerName;
		LuaObject.checkType(l, 1, out layerName);
		if (string.IsNullOrEmpty(layerName))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'GetCollisionLayerId'");
		}

		LuaDLL.lua_pushinteger(l, LayerMask.NameToLayer(layerName));
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int GetSortingLayerId(IntPtr l)
	{
		string layerName;
		LuaObject.checkType(l, 1, out layerName);
		if (string.IsNullOrEmpty(layerName))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'GetSortingLayerId'");
		}

		LuaDLL.lua_pushinteger(l, SortingLayer.NameToID(layerName));
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetCollisionGroup(IntPtr l)
	{
		var ignore = false;
		if (LuaDLL.lua_gettop(l) >= 3)
		{
			LuaObject.checkType(l, 3, out ignore);
			ignore = !ignore;
		}
		
		Physics.IgnoreLayerCollision(LuaDLL.luaL_checkinteger(l, 1), LuaDLL.luaL_checkinteger(l, 2), ignore);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int ShouldCollideWith(IntPtr l)
	{
		LuaDLL.lua_pushboolean(l,
			!Physics.GetIgnoreLayerCollision(LuaDLL.luaL_checkinteger(l, 1), LuaDLL.luaL_checkinteger(l, 2)));
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int CreateUI(IntPtr l)
	{
		string name, path;
		LuaObject.checkType(l, 1, out name);
		LuaObject.checkType(l, 2, out path);

		if (name == null || path == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'CreateUI'");
		}

		var json = Game.GameInstance.ResourceManager.FindResourceAs<ResText>(name, path);

		var uiObj = new GameObject(name)
		{
			layer = LayerMask.NameToLayer("UI")
		};
		var ui = uiObj.AddComponent<JsonUI>();
		ui.OnAcquireJson(json.GetContent());

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int CreateUIFromContent(IntPtr l)
	{
		string name, content;
		LuaObject.checkType(l, 1, out name);
		LuaObject.checkType(l, 2, out content);

		if (name == null || content == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'CreateUI'");
		}

		var uiObj = new GameObject(name)
		{
			layer = LayerMask.NameToLayer("UI")
		};
		var ui = uiObj.AddComponent<JsonUI>();
		ui.OnAcquireJson(content);

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetUI(IntPtr l)
	{
		string name;
		LuaTable content;
		LuaObject.checkType(l, 1, out name);
		LuaObject.checkType(l, 2, out content);

		if (name == null || content == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'SetUINode'");
		}

		var uiObj = GameObject.Find(name);
		JsonUI ui;
		if (uiObj == null || (ui = uiObj.GetComponent<JsonUI>()) == null)
		{
			return LuaDLL.luaL_error(l, "no such ui.");
		}

		using (content)
		{
			var jobj = ui.GetJObject();
			jobj.RemoveAll();
			foreach (var pair in content)
			{
				var obj = new JObject();
				foreach (var item in (LuaTable) pair.value)
				{
					obj.Add((string) item.key, JToken.FromObject(item.value));
				}
				jobj.Add((string) pair.key, obj);
			}
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int GetUI(IntPtr l)
	{
		string name;
		LuaObject.checkType(l, 1, out name);

		if (name == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'DestroyUI'");
		}

		var uiObj = GameObject.Find(name);
		JsonUI ui;
		if (uiObj == null || (ui = uiObj.GetComponent<JsonUI>()) == null)
		{
			return LuaDLL.luaL_error(l, "no such ui.");
		}

		var jobj = ui.GetJObject();
		LuaDLL.lua_createtable(l, 0, jobj.Count);
		foreach (var node in jobj)
		{
			var values = (JObject) node.Value;
			LuaDLL.lua_createtable(l, 0, values.Count);
			foreach (var item in values)
			{
				var str = item.Value.ToString();
				int testint;
				float testfloat;
				if (int.TryParse(str, out testint))
				{
					LuaDLL.lua_pushinteger(l, testint);
				}
				else if (float.TryParse(str, out testfloat))
				{
					LuaDLL.lua_pushnumber(l, testfloat);
				}
				else
				{
					LuaDLL.lua_pushstring(l, str);
				}
				LuaDLL.lua_setfield(l, -2, item.Key);
			}
			LuaDLL.lua_setfield(l, -2, node.Key);
		}

		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetUINode(IntPtr l)
	{
		string name, nodename;
		LuaTable content;
		LuaObject.checkType(l, 1, out name);
		LuaObject.checkType(l, 2, out nodename);
		LuaObject.checkType(l, 3, out content);

		if (name == null || nodename == null || content == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'SetUINode'");
		}

		var uiObj = GameObject.Find(name);
		JsonUI ui;
		if (uiObj == null || (ui = uiObj.GetComponent<JsonUI>()) == null)
		{
			return LuaDLL.luaL_error(l, "no such ui.");
		}
		
		using (content)
		{
			JToken token;
			JObject node;
			if (ui.GetJObject().TryGetValue(nodename, out token))
			{
				node = (JObject) token;
			}
			else
			{
				node = new JObject();
				ui.GetJObject().Add(nodename, node);
			}
			
			foreach (var item in content)
			{
				node[(string) item.key] = JToken.FromObject(item.value);
			}
		}
		
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int GetUINode(IntPtr l)
	{
		string name, nodename;
		LuaObject.checkType(l, 1, out name);
		LuaObject.checkType(l, 2, out nodename);

		if (name == null || nodename == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'DestroyUI'");
		}

		var uiObj = GameObject.Find(name);
		JsonUI ui;
		if (uiObj == null || (ui = uiObj.GetComponent<JsonUI>()) == null)
		{
			return LuaDLL.luaL_error(l, "no such ui.");
		}
		
		JToken token;
		if (!ui.GetJObject().TryGetValue(nodename, out token))
		{
			LuaDLL.lua_pushnil(l);
		}
		else
		{
			var node = (JObject) token;
			LuaDLL.lua_createtable(l, 0, node.Count);
			foreach (var item in node)
			{
				var str = item.Value.ToString();
				int testint;
				float testfloat;
				if (int.TryParse(str, out testint))
				{
					LuaDLL.lua_pushinteger(l, testint);
				}
				else if (float.TryParse(str, out testfloat))
				{
					LuaDLL.lua_pushnumber(l, testfloat);
				}
				else
				{
					LuaDLL.lua_pushstring(l, str);
				}
				LuaDLL.lua_setfield(l, -2, item.Key);
			}
		}
		
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int RemoveUINode(IntPtr l)
	{
		string name, nodename;
		LuaObject.checkType(l, 1, out name);
		LuaObject.checkType(l, 2, out nodename);

		if (name == null || nodename == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'DestroyUI'");
		}

		var uiObj = GameObject.Find(name);
		JsonUI ui;
		if (uiObj == null || (ui = uiObj.GetComponent<JsonUI>()) == null)
		{
			return LuaDLL.luaL_error(l, "no such ui.");
		}

		ui.GetJObject().Remove(nodename);

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int DestroyUI(IntPtr l)
	{
		string name;
		LuaObject.checkType(l, 1, out name);

		if (name == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'DestroyUI'");
		}

		var uiObj = GameObject.Find(name);
		if (uiObj != null && uiObj.GetComponent<JsonUI>() != null)
		{
			Object.Destroy(uiObj);
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int LoadTexture(IntPtr l)
	{
		string name, path;
		LuaObject.checkType(l, 1, out name);
		LuaObject.checkType(l, 2, out path);

		var activedPool = Game.GameInstance.ResourceManager.GetActivedPool();
		if (activedPool == null)
		{
			return LuaDLL.luaL_error(l, "cannot load resource at this time.");
		}

		if (activedPool.GetResourceAs<ResTexture>(name, path) == null)
		{
			return LuaDLL.luaL_error(l, "cannot load texture from path '{1}' as name '{0}'", name, path);
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int GetTextureSize(IntPtr l)
	{
		string name;
		LuaObject.checkType(l, 1, out name);

		var texture = Game.GameInstance.ResourceManager.FindResourceAs<ResTexture>(name, autoLoad: false);

		if (texture == null)
		{
			return LuaDLL.luaL_error(l, "texture '{0}' does not exist.", name);
		}

		var tex = texture.GetTexture();
		LuaDLL.lua_pushnumber(l, tex.width);
		LuaDLL.lua_pushnumber(l, tex.height);
		return 2;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int LoadImage(IntPtr l)
	{
		var top = LuaDLL.lua_gettop(l);

		string name, textureName;
		LuaObject.checkType(l, 1, out name);
		LuaObject.checkType(l, 2, out textureName);

		var activedPool = Game.GameInstance.ResourceManager.GetActivedPool();
		if (activedPool == null)
		{
			return LuaDLL.luaL_error(l, "cannot load resource at this time.");
		}

		if (activedPool.ResourceExists(name, typeof(ResSprite)))
		{
			return LuaDLL.luaL_error(l, "sprite '{0}' has already loaded", name);
		}

		if (!activedPool.ResourceExists(textureName, typeof(ResTexture)))
		{
			return LuaDLL.luaL_error(l, "texture '{0}' has not loaded", textureName);
		}

		var resTexture = activedPool.GetResourceAs<ResTexture>(textureName);
		var texture = resTexture.GetTexture();
		var spriteRect = new Rect(
			(float) LuaDLL.luaL_checknumber(l, 3),
			(float) LuaDLL.luaL_checknumber(l, 4),
			(float) LuaDLL.luaL_checknumber(l, 5),
			(float) LuaDLL.luaL_checknumber(l, 6));
		// fancy2D的精灵的原点在左上角，Unity的精灵的原点在左下角
		spriteRect.y = texture.height - spriteRect.y - spriteRect.height;
		var sprite = Sprite.Create(texture, spriteRect, new Vector2(0.5f, 0.5f));
		
		float a = 0, b = 0;
		var rect = false;

		if (top >= 7)
		{
			LuaObject.checkType(l, 7, out a);
		}

		if (top >= 8)
		{
			LuaObject.checkType(l, 8, out b);
		}

		if (top >= 9)
		{
			LuaObject.checkType(l, 9, out rect);
		}

		if (!activedPool.AddResource(new ResSprite(name, sprite, a, b, rect)))
		{
			return LuaDLL.luaL_error(l, "some error occured while adding sprite '{0}' to resource pool", name);
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int LoadAnimation(IntPtr l)
	{
		var top = LuaDLL.lua_gettop(l);

		string name, textureName;
		LuaObject.checkType(l, 1, out name);
		LuaObject.checkType(l, 2, out textureName);

		var activedPool = Game.GameInstance.ResourceManager.GetActivedPool();
		if (activedPool == null)
		{
			return LuaDLL.luaL_error(l, "cannot load resource at this time.");
		}

		if (activedPool.ResourceExists(name, typeof(ResAnimation)))
		{
			return LuaDLL.luaL_error(l, "sprite '{0}' has already loaded", name);
		}

		if (!activedPool.ResourceExists(textureName, typeof(ResTexture)))
		{
			return LuaDLL.luaL_error(l, "texture '{0}' has not loaded", textureName);
		}

		float a = 0, b = 0;
		var rect = false;

		if (top >= 10)
		{
			LuaObject.checkType(l, 10, out a);
		}

		if (top >= 11)
		{
			LuaObject.checkType(l, 11, out b);
		}

		if (top >= 12)
		{
			LuaObject.checkType(l, 12, out rect);
		}

		var ani = new ResAnimation(name, activedPool.GetResourceAs<ResTexture>(textureName, autoLoad: false),
			(float) LuaDLL.luaL_checknumber(l, 3), (float) LuaDLL.luaL_checknumber(l, 4), (float) LuaDLL.luaL_checknumber(l, 5),
			(float) LuaDLL.luaL_checknumber(l, 6), (uint) LuaDLL.luaL_checkinteger(l, 7), (uint) LuaDLL.luaL_checkinteger(l, 8),
			(uint) LuaDLL.luaL_checkinteger(l, 9), a, b, rect);

		return activedPool.AddResource(ani) ? 0 : LuaDLL.luaL_error(l, "load animation failed (name='{0}', tex='{1}').", name, textureName);
	}

	// TODO: 需要修正
	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int LoadPS(IntPtr l)
	{
		var top = LuaDLL.lua_gettop(l);
		string name, path, imgName;

		if (!LuaObject.checkType(l, 1, out name) || !LuaObject.checkType(l, 2, out path) || !LuaObject.checkType(l, 3, out imgName))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'LoadPS'");
		}

		float a = 0, b = 0;
		var rect = true;

		if (top >= 4)
		{
			LuaObject.checkType(l, 4, out a);
		}

		if (top >= 5)
		{
			LuaObject.checkType(l, 5, out b);
		}

		if (top >= 6)
		{
			LuaObject.checkType(l, 6, out rect);
		}

		var activedPool = Game.GameInstance.ResourceManager.GetActivedPool();
		if (!activedPool.ResourceExists(imgName, typeof(ResSprite)))
		{
			return LuaDLL.luaL_error(l, "sprite '{0}' has not loaded.", imgName);
		}
		if (activedPool.ResourceExists(name, typeof(ResParticle)))
		{
			return LuaDLL.luaL_error(l, "particle {0} has already loaded.", name);
		}

		var sprite = activedPool.GetResourceAs<ResSprite>(imgName);
		var particle = activedPool.GetResourceAs<ResParticle>(name, path);
		particle.Ab = new Vector2(a, b);
		particle.Rect = rect;
		var realSprite = sprite.GetSprite();
		var newTexture = new Texture2D((int) realSprite.textureRect.width, (int) realSprite.textureRect.height);
		var pixels = realSprite.texture.GetPixels((int) realSprite.textureRect.x,
			(int) realSprite.textureRect.y,
			(int) realSprite.textureRect.width,
			(int) realSprite.textureRect.height);
		newTexture.SetPixels(pixels);
		newTexture.Resize(newTexture.width / 5, newTexture.height / 5);
		newTexture.Apply();
		particle.SetMaterial(new Material(Shader.Find("Particles/Alpha Blended Premultiply"))
		{
			mainTexture = newTexture
		});
		
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int ParticleFire(IntPtr l)
	{
		if (!LuaDLL.lua_istable(l, 1))
			return LuaDLL.luaL_error(l, "invalid lstg object for 'ParticleFire'.");
		LuaDLL.lua_rawgeti(l, 1, 2);
		
		var obj = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -1));
		if (obj == null)
		{
			return LuaDLL.luaL_error(l, "invalid lstg object for 'ParticleFire'.");
		}

		var particleSys = obj.GetComponent<ParticleSystem>();
		if (particleSys)
		{
			particleSys.Play();
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int ParticleStop(IntPtr l)
	{
		if (!LuaDLL.lua_istable(l, 1))
			return LuaDLL.luaL_error(l, "invalid lstg object for 'ParticleStop'.");
		LuaDLL.lua_rawgeti(l, 1, 2);

		var obj = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -1));
		if (obj == null)
		{
			return LuaDLL.luaL_error(l, "invalid lstg object for 'ParticleStop'.");
		}

		var particleSys = obj.GetComponent<ParticleSystem>();
		if (particleSys)
		{
			particleSys.Stop();
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int ParticleGetn(IntPtr l)
	{
		if (!LuaDLL.lua_istable(l, 1))
			return LuaDLL.luaL_error(l, "invalid lstg object for 'ParticleGetn'.");
		LuaDLL.lua_rawgeti(l, 1, 2);

		var obj = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -1));
		if (obj == null)
		{
			return LuaDLL.luaL_error(l, "invalid lstg object for 'ParticleGetn'.");
		}

		var particleSys = obj.GetComponent<ParticleSystem>();
		if (particleSys)
		{
			LuaDLL.lua_pushinteger(l, particleSys.particleCount);
		}
		else
		{
			return LuaDLL.luaL_error(l, "no particle system in this lstg object.");
		}

		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int ParticleGetEmission(IntPtr l)
	{
		if (!LuaDLL.lua_istable(l, 1))
			return LuaDLL.luaL_error(l, "invalid lstg object for 'ParticleGetEmission'.");
		LuaDLL.lua_rawgeti(l, 1, 2);

		var obj = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -1));
		if (obj == null)
		{
			return LuaDLL.luaL_error(l, "invalid lstg object for 'ParticleGetEmission'.");
		}

		var particleSys = obj.GetComponent<ParticleSystem>();
		if (particleSys)
		{
			LuaDLL.lua_pushnumber(l, particleSys.emission.rateOverTime.constant);
		}
		else
		{
			return LuaDLL.luaL_error(l, "no particle system in this lstg object.");
		}

		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int ParticleSetEmission(IntPtr l)
	{
		if (!LuaDLL.lua_istable(l, 1))
			return LuaDLL.luaL_error(l, "invalid lstg object for 'ParticleSetEmission'.");
		LuaDLL.lua_rawgeti(l, 1, 2);

		var obj = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -1));
		if (obj == null)
		{
			return LuaDLL.luaL_error(l, "invalid lstg object for 'ParticleSetEmission'.");
		}

		var particleSys = obj.GetComponent<ParticleSystem>();
		if (particleSys)
		{
			var emission = particleSys.emission;
			emission.rateOverTime = new ParticleSystem.MinMaxCurve((float) LuaDLL.luaL_checknumber(l, 2));
		}
		else
		{
			return LuaDLL.luaL_error(l, "no particle system in this lstg object.");
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int LoadAudio(IntPtr l)
	{
		return 0;
		/*string name, path;
		LuaObject.checkType(l, 1, out name);
		LuaObject.checkType(l, 2, out path);

		var activedPool = Game.GameInstance.ResourceManager.GetActivedPool();
		if (activedPool.ResourceExists(name, typeof(ResAudio)))
		{
			return LuaDLL.luaL_error(l, "audio '{0}' has already loaded.", name);
		}

		return activedPool.GetResourceAs<ResAudio>(name, path) == null ? LuaDLL.luaL_error(l, "failed to load audio {0} from path {1}.", name, path) : 0;*/
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetAudioLoop(IntPtr l)
	{
		return 0;
		/*string name;
		float loopBegin, loopEnd;
		LuaObject.checkType(l, 1, out name);
		if (string.IsNullOrEmpty(name) || !LuaObject.checkType(l, 2, out loopBegin) || !LuaObject.checkType(l, 3, out loopEnd))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'SetAudioLoop'");
		}

		if (!Game.GameInstance.ResourceManager.ResourceExists(name, typeof(ResAudio)))
		{
			return LuaDLL.luaL_error(l, "audio '{0}' does not exist.", name);
		}

		var audio = Game.GameInstance.ResourceManager.FindResourceAs<ResAudio>(name, autoLoad: false);
		if (audio == null)
		{
			return LuaDLL.luaL_error(l, "internal error occured while trying to load audio '{0}'.", name);
		}

		audio.SetLoopInfo(loopBegin, loopEnd);

		return 0;*/
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int PlayMusic(IntPtr l)
	{
		/*var argc = LuaDLL.lua_gettop(l);
		string name;
		float volume = 1, position = 0;

		LuaObject.checkType(l, 1, out name);
		if (string.IsNullOrEmpty(name))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'PlayMusic'");
		}
		
		var audio = Game.GameInstance.ResourceManager.FindResourceAs<ResAudio>(name, autoLoad: false);
		if (audio == null)
		{
			return LuaDLL.luaL_error(l, "cannot load audio '{0}'", name);
		}

		var audioClip = audio.GetAudioClip();
		if (!audioClip.LoadAudioData())
		{
			return LuaDLL.luaL_error(l, "cannot load audio '{0}'", name);
		}

		if (argc >= 2)
		{
			LuaObject.checkType(l, 2, out volume);
		}

		if (argc >= 3)
		{
			LuaObject.checkType(l, 3, out position);
		}

		Game.GameInstance.MusicAudioSource.Stop();
		Game.GameInstance.MusicAudioSource.clip = audioClip;
		Game.GameInstance.MusicAudioSource.time = position;
		Game.GameInstance.MusicAudioSource.volume = volume * Game.GameInstance.GlobalMusicVolume;
		Game.GameInstance.MusicAudioSource.Play();*/

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int PlaySound(IntPtr l)
	{
		/*var argc = LuaDLL.lua_gettop(l);
		string name;
		float volume = 1, pan = 0;

		LuaObject.checkType(l, 1, out name);
		if (string.IsNullOrEmpty(name))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'PlaySound'");
		}

		var audio = Game.GameInstance.ResourceManager.FindResourceAs<ResAudio>(name, autoLoad: false);
		if (audio == null)
		{
			return LuaDLL.luaL_error(l, "cannot load audio '{0}'", name);
		}

		var audioClip = audio.GetAudioClip();
		if (!audioClip.LoadAudioData())
		{
			return LuaDLL.luaL_error(l, "cannot load audio '{0}'", name);
		}

		if (argc >= 2)
		{
			LuaObject.checkType(l, 2, out volume);
		}

		if (argc >= 3)
		{
			LuaObject.checkType(l, 3, out pan);
		}

		Game.GameInstance.SoundAudioSource.Stop();
		Game.GameInstance.SoundAudioSource.clip = audioClip;
		Game.GameInstance.SoundAudioSource.panStereo = pan;
		Game.GameInstance.SoundAudioSource.volume = volume * Game.GameInstance.GlobalSoundEffectVolume;
		Game.GameInstance.SoundAudioSource.Play();*/

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetImageScale(IntPtr l)
	{
		Game.GameInstance.GlobalImageScaleFactor = (float) LuaDLL.luaL_checknumber(l, 1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int GetAttr(IntPtr l)
	{
		LuaDLL.lua_rawgeti(l, 1, 2);
		var id = LuaDLL.lua_tointeger(l, -1);
		LuaDLL.lua_pop(l, 1);

		var obj = Game.GameInstance.GetObject(id);

		string key;
		LuaObject.checkType(l, -1, out key);
		if (key == null)
		{
			return LuaDLL.luaL_error(l, "invalid key for 'GetAttr'");
		}
		
		var match = AttrRegex.Match(key);
		if (!match.Success)
		{
			return LuaDLL.luaL_error(l, "key '{0}' is invalid", key);
		}
		var propName = match.Groups[1].Value;
		var optDimension = match.Groups[2].Value;
		
		var noAliasProperty = false;
		var prop = LSTGObject.FindProperty(propName);

		try
		{
			if (prop != null)
			{
				var value = prop.GetValue(obj, null);
				switch (optDimension)
				{
					case "x":
						LuaObject.pushValue(l, ((Vector2)value).x);
						break;
					case "y":
						LuaObject.pushValue(l, ((Vector2)value).y);
						break;
					case "":
						LuaObject.pushValue(l, value);
						break;
					default:
						noAliasProperty = true;
						break;
				}
			}

			if (prop == null || noAliasProperty)
			{
				switch (key)
				{
					case "id":
						LuaObject.pushValue(l, obj.Id);
						break;
					case "a":
						LuaObject.pushValue(l, obj.Ab.x);
						break;
					case "b":
						LuaObject.pushValue(l, obj.Ab.y);
						break;
					case "hscale":
						LuaObject.pushValue(l, obj.Scale.x);
						break;
					case "vscale":
						LuaObject.pushValue(l, obj.Scale.y);
						break;
					case "class":
						LuaDLL.lua_rawgeti(l, 1, 1);
						break;
					case "img":
						LuaObject.pushValue(l, obj.RenderResource == null ? null : obj.RenderResource.GetName());
						break;
					default:
						LuaDLL.lua_pushnil(l);
						break;
				}
			}
		}
		catch (Exception e)
		{
			return LuaDLL.luaL_error(l, "some error occured while trying to get key '{0}', exception is {1}", key, e);
		}

		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetAttr(IntPtr l)
	{
		LuaDLL.lua_rawgeti(l, 1, 2);
		var id = LuaDLL.lua_tointeger(l, -1);
		LuaDLL.lua_pop(l, 1);

		var obj = Game.GameInstance.GetObject(id);

		string key;
		LuaObject.checkType(l, -2, out key);
		if (key == null)
		{
			return LuaDLL.luaL_error(l, "invalid key for 'GetAttr'");
		}
			
		var match = AttrRegex.Match(key);
		if (!match.Success)
		{
			return LuaDLL.luaL_error(l, "key '{0}' is invalid", key);
		}
		var propName = match.Groups[1].Value;

		var noAliasProperty = false;
		var prop = LSTGObject.FindProperty(propName);
		try
		{
			if (prop != null)
			{
				var setter = prop.GetSetMethod();
				if (setter == null)
				{
					return LuaDLL.luaL_error(l, "key '{0}' is readonly", key);
				}
				var value = prop.GetValue(obj, null);
				switch (match.Groups[2].Value)
				{
					case "x":
					{
						var vec = (Vector2)value;
						vec.x = (float)LuaDLL.luaL_checknumber(l, -1);
						prop.SetValue(obj, vec, null);
					}
						break;
					case "y":
					{
						var vec = (Vector2)value;
						vec.y = (float)LuaDLL.luaL_checknumber(l, -1);
						prop.SetValue(obj, vec, null);
					}
						break;
					case "":
						try
						{
							prop.SetValue(obj, Convert.ChangeType(LuaObject.checkVar(l, -1), prop.PropertyType), null);
						}
						catch (Exception)
						{
							goto default;
						}
						break;
					default:
						noAliasProperty = true;
						break;
				}
			}

			if (prop == null || noAliasProperty)
			{
				switch (key)
				{
					case "a":
					{
						var ab = obj.Ab;
						ab.x = (float)LuaDLL.luaL_checknumber(l, -1);
						obj.Ab = ab;
					}
						break;
					case "b":
					{
						var ab = obj.Ab;
						ab.y = (float)LuaDLL.luaL_checknumber(l, -1);
						obj.Ab = ab;
					}
						break;
					case "hscale":
					{
						var scale = obj.Scale;
						scale.x = (float)LuaDLL.luaL_checknumber(l, -1);
						obj.Scale = scale;
					}
						break;
					case "vscale":
					{
						var scale = obj.Scale;
						scale.y = (float)LuaDLL.luaL_checknumber(l, -1);
						obj.Scale = scale;
					}
						break;
					case "class":
						LuaDLL.lua_rawseti(l, 1, 1);
						break;
					case "img":
					{
						string resName;
						LuaObject.checkType(l, -1, out resName);
						if (string.IsNullOrEmpty(resName))
						{
							return LuaDLL.luaL_error(l, "invalid img");
						}
						obj.RenderResource = Game.GameInstance.ResourceManager.FindResource(resName);
						obj.transform.localScale = new Vector3(Game.GameInstance.GlobalImageScaleFactor,
							Game.GameInstance.GlobalImageScaleFactor, Game.GameInstance.GlobalImageScaleFactor);
					}
						break;
					default:
						LuaDLL.lua_rawset(l, 1);
						break;
				}
			}
		}
		catch (Exception e)
		{
			return LuaDLL.luaL_error(l, "some error occured while trying to set key '{0}', exception is {1}", key, e);
		}
			
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetBound(IntPtr l)
	{
		float left, right, bottom, top;

		if (!LuaObject.checkType(l, 1, out left) ||
		    !LuaObject.checkType(l, 2, out right) ||
		    !LuaObject.checkType(l, 3, out bottom) ||
		    !LuaObject.checkType(l, 4, out top))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'SetBound'");
		}

		var bound = Game.GameInstance.Bound;
		bound.center = new Vector2((left + right) / 2.0f, (bottom + top) / 2.0f);
		bound.size = new Vector3(right - left, top - bottom, 0.2f);

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int DefaultRenderFunc(IntPtr l)
	{
		LuaDLL.lua_rawgeti(l, -1, 2);
		int objIndex;
		if (!LuaObject.checkType(l, -1, out objIndex))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'DefaultRenderFunc'");
		}
		var obj = Game.GameInstance.GetObject(objIndex);
		if (obj != null && obj)
		{
			obj.DefaultRenderFunc();
		}
		LuaDLL.lua_pop(l, 1);

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int ObjList(IntPtr l)
	{
		var group = LuaDLL.luaL_checkinteger(l, 1);
		if (group != -1 && LayerMask.LayerToName(group) == null)
		{
			group = -1;
		}
		var list = Game.GameInstance.GetObjects().Where(o => group == -1 || o.Group == group).ToList();
		var e = (IEnumerable<LSTGObject>) list;
		var iterator = e.GetEnumerator();
		
		LuaDLL.lua_pushcclosure(l, l_ =>
		{
			if (iterator != null)
			{
				if (iterator.MoveNext())
				{
					var obj = iterator.Current;
					if (obj != null)
					{
						LuaDLL.lua_pushinteger(l_, obj.Id + 1);
						LuaObject.pushValue(l_, obj.ObjTable);
						return 2;
					}
				}

				iterator.Dispose();
				iterator = null;
			}
			
			return 0;
		}, 0);
		/*LuaDLL.lua_pushinteger(l, group);
		if (!iterator.MoveNext())
		{
			LuaDLL.lua_pushinteger(l, -1);
		}
		else
		{
			System.Diagnostics.Debug.Assert(iterator.Current != null, "iterator.Current != null");
			LuaDLL.lua_pushinteger(l, iterator.Current.Id);
		}*/
		
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	[LuaFunctionAliasAs("GetnObj")]
	public static int GetObjectCount(IntPtr l)
	{
		LuaDLL.lua_pushinteger(l, Game.GameInstance.ObjectCount);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int UpdateObjList(IntPtr l)
	{
		// 已否决
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int ObjFrame(IntPtr l)
	{
		// 已否决
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int ObjRender(IntPtr l)
	{
		// 已否决
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int New(IntPtr l)
	{
		return Game.GameInstance.NewObject(l);
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int Del(IntPtr l)
	{
		LuaDLL.lua_rawgeti(l, 1, 2);
		var id = LuaDLL.lua_tointeger(l, -1);
		LuaDLL.lua_pop(l, 1);

		var obj = Game.GameInstance.GetObject(id);
		if (obj != null)
		{
			obj.DelSelf();
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int Kill(IntPtr l)
	{
		LuaDLL.lua_rawgeti(l, 1, 2);
		var id = LuaDLL.lua_tointeger(l, -1);
		LuaDLL.lua_pop(l, 1);

		var obj = Game.GameInstance.GetObject(id);
		if (obj != null)
		{
			obj.KillSelf();
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int IsValid(IntPtr l)
	{
		var argc = LuaDLL.lua_gettop(l);
		LuaDLL.lua_pushboolean(l, Enumerable.Range(1, argc).Select(i =>
		{
			if (!LuaDLL.lua_istable(l, i))
			{
				return false;
			}
			LuaDLL.lua_rawgeti(l, i, 2);
			if (!LuaDLL.lua_isnumber(l, -1))
			{
				return false;
			}
			var id = LuaDLL.lua_tointeger(l, -1);
			LuaDLL.lua_pop(l, 1);
			return Game.GameInstance.GetObject(id) != null;
		}).All(v => v));
		
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetViewport(IntPtr l)
	{
		var left = (float) LuaDLL.luaL_checknumber(l, 1);
		var right = (float) LuaDLL.luaL_checknumber(l, 2);
		var bottom = (float) LuaDLL.luaL_checknumber(l, 3);
		var top = (float) LuaDLL.luaL_checknumber(l, 4);

		Camera.main.rect = new Rect(left, bottom, right - left, top - bottom);

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetFog(IntPtr l)
	{
		if (LuaDLL.lua_gettop(l) == 0)
		{
			RenderSettings.fog = false;
		}
		else
		{
			RenderSettings.fog = true;
			RenderSettings.fogStartDistance = (float) LuaDLL.luaL_checknumber(l, 1);
			RenderSettings.fogEndDistance = (float) LuaDLL.luaL_checknumber(l, 2);

			Color color;
			if (LuaDLL.lua_gettop(l) <= 2 || !LuaObject.checkType(l, 3, out color))
			{
				color = Color.white;
			}
			RenderSettings.fogColor = color;
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetOrtho(IntPtr l)
	{
		Camera.main.orthographic = true;
		Camera.main.worldToCameraMatrix = Matrix4x4.identity;
		Camera.main.cullingMatrix = Matrix4x4.identity;
		Camera.main.projectionMatrix = Matrix4x4.Ortho(
			(float) LuaDLL.luaL_checknumber(l, 1),
			(float) LuaDLL.luaL_checknumber(l, 2),
			(float) LuaDLL.luaL_checknumber(l, 3),
			(float) LuaDLL.luaL_checknumber(l, 4), 0, 100);

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetPerspective(IntPtr l)
	{
		// TODO
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetImgState(IntPtr l)
	{
		LuaTable obj;
		LuaObject.checkType(l, 1, out obj);
		if (obj == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'SetImgState'");
		}
		var id = (int) obj[2];
		var blendMode = TranslateBlendMode(l, 2);
		// TODO

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetImageState(IntPtr l)
	{
		string image;
		LuaObject.checkType(l, 1, out image);
		if (string.IsNullOrEmpty(image))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'SetImageState'");
		}
		var sprite = Game.GameInstance.ResourceManager.FindResourceAs<ResSprite>(image, autoLoad: false);
		if (sprite == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'SetImageState'");
		}

		var blendMode = TranslateBlendMode(l, 2);
		var top = LuaDLL.lua_gettop(l);
		if (top == 3)
		{
			// TODO
		}
		else if (top == 6)
		{
			
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetImageCenter(IntPtr l)
	{
		string image;
		LuaObject.checkType(l, 1, out image);
		if (string.IsNullOrEmpty(image))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'SetImageCenter'");
		}
		var sprite = Game.GameInstance.ResourceManager.FindResourceAs<ResSprite>(image, autoLoad: false);
		if (sprite == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'SetImageCenter'");
		}
		
		// TODO

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int SetAnimationState(IntPtr l)
	{
		// TODO

		return 0;
	}

	/*[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int Render(IntPtr l)
	{
		string textureName;
		LuaObject.checkType(l, 1, out textureName);
		if (string.IsNullOrEmpty(textureName))
		{
			return LuaDLL.luaL_error(l, "invalid argument for 'Render'");
		}



		return 0;
	}*/

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	public static int DebugHint(IntPtr l)
	{
		System.Diagnostics.Debugger.Break();
		UnityEditor.EditorApplication.isPaused = true;
		return 0;
	}

	internal static BlendMode TranslateBlendMode(IntPtr l, int argnum)
	{
		string s;
		LuaObject.checkType(l, argnum, out s);
		if (s != null)
		{
			switch (s)
			{
				case "mul+add":
					return BlendMode.MulAdd;
				case "":
				case "mul+alpha":
					return BlendMode.MulAlpha;
				case "add+add":
					return BlendMode.AddAdd;
				case "add+alpha":
					return BlendMode.AddAlpha;
				case "add+rev":
					return BlendMode.AddRev;
				case "mul+rev":
					return BlendMode.MulRev;
				case "add+sub":
					return BlendMode.AddSub;
				case "mul+sub":
					return BlendMode.MulSub;
				default:
					LuaDLL.luaL_error(l, "invalid blend mode '{0}'.", s);
					break;
			}
		}

		return BlendMode.MulAlpha;
	}

	public static void Register(IntPtr l)
	{
		foreach (var method in from method in typeof(BuiltinFunctions).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where method.IsDefined(typeof(MonoPInvokeCallbackAttribute), false)
			select method)
		{
			var aliasAttr = (LuaFunctionAliasAsAttribute) method.GetCustomAttributes(typeof(LuaFunctionAliasAsAttribute), false).FirstOrDefault();
			LuaObject.reg(l, (LuaCSFunction) Delegate.CreateDelegate(typeof(LuaCSFunction), method), "lstg", aliasAttr == null ? null : aliasAttr.Alias);
		}
	}

	public static void InitMetaTable(IntPtr l)
	{
		LuaDLL.lua_pushlightuserdata(l, Game.GameInstance.LuaVM.luaState.L);
		LuaDLL.lua_createtable(l, Game.MaxObjectCount, 0);

		LuaDLL.lua_newtable(l);
		LuaDLL.lua_getglobal(l, "lstg");
		LuaDLL.lua_pushstring(l, "GetAttr");
		LuaDLL.lua_gettable(l, -2);
		LuaDLL.lua_pushstring(l, "SetAttr");
		LuaDLL.lua_gettable(l, -3);
		//Debug.Assert(LuaDLL.lua_iscfunction(l, -1) && LuaDLL.lua_iscfunction(l, -2));
		LuaDLL.lua_setfield(l, -4, "__newindex");
		LuaDLL.lua_setfield(l, -3, "__index");
		LuaDLL.lua_pop(l, 1);

		LuaDLL.lua_setfield(l, -2, LSTGObject.ObjMetadataTableName);
		LuaDLL.lua_settable(l, LuaIndexes.LUA_REGISTRYINDEX);
	}
}

[CustomLuaClass]
public sealed class Rand
{
	private Random _rand;

	public void Seed(int seed)
	{
		_rand = new Random(seed);
	}

	public int Int(int a, int b)
	{
		return _rand.Next(a, b + 1);
	}

	public float Float(float a, float b)
	{
		return (b - a) * (float) _rand.NextDouble() + a;
	}

	public int Sign()
	{
		return _rand.Next(0, 2) * 2 - 1;
	}

	public override string ToString()
	{
		return "lstg.Rand object";
	}
}
