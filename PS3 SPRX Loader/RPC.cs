﻿using System.Threading;

namespace PS3_SPRX_Loader {
    public class RPC {
        private static TMAPI PS3;

        private const uint ENABLE_ADDR =        0x10060000;
        private const uint MODULE_IDS_ADDR =    0x10060004;
        private const uint MODULE_ERROR_ADDR =  0x10060008;
        private const uint PLUGIN_PATH_ADDR =   0x10060010;

        public static bool RPC_ENABLED = false;
        private static uint RPC_INSTALL_ADDR = 0x0;

        private static byte[] RESTORE_0 = new byte[0x4];
        private static byte[] RESTORE_BYTES = new byte[0x1DC];

        private static byte[] RPC_BLR = new byte[] {
            0x4E, 0x80, 0x00, 0x20 //blr
        };

        private static byte[] RPC_ADJUST_STACK = new byte[] {
            0xF8, 0x21, 0xFF, 0x91 //stdu r1, -0x70(r1)
        };

        private static byte[] RPC_BYTES = new byte[] {
            0x7C, 0x08, 0x02, 0xA6, //mflr r0
            0xF8, 0x01, 0x00, 0x80, //std r0, 0x80(%r1)
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x80, 0x64, 0x00, 0x00, //lwz %r3, 0(%r4)
            0x2C, 0x03, 0x00, 0x01, //cmpwi %r3, 1
            0x40, 0x82, 0x00, 0xD4, //bne 0xD4
            0x38, 0x60, 0x00, 0x00, //li %r3, 0x0
            0x90, 0x64, 0x00, 0x00, //stw %r3, 0(%r4)
            0x3C, 0x60, 0x10, 0x06, //lis %r3, 0x1006
            0x60, 0x63, 0x00, 0x10, //ori %r3, %r3, 0x10
            0x38, 0x80, 0x00, 0x00, //li %r4, 0
            0x38, 0xA0, 0x00, 0x00, //li %r5, 0
            0x39, 0x60, 0x01, 0xE0, //li %r11, 0x1E0
            0x44, 0x00, 0x00, 0x02, //;sc                   #SYS_PRX_LOAD_MODULE(const char* filePath, sys_prx_flags_t flags, sys_prx_load_module_option_t* moduleOptions)
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x80, 0x01, 0x88, //blt 0x188
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x90, 0x64, 0x00, 0x04, //stw %r3, 0x4(%r4)
            0x38, 0x80, 0x00, 0x28, //li %r4, 0x28
            0x38, 0xA0, 0x00, 0x01, //li %r5, 0x1
            0x38, 0xE0, 0x00, 0x00, //li %r7, 0
            0x38, 0xC0, 0xFF, 0xFF, //li %r6, -1
            0xF8, 0x81, 0x00, 0x30, //std %r4, 0x30(%r1)
            0xF8, 0xA1, 0x00, 0x38, //std %r5, 0x38(%r1)
            0xF8, 0xE1, 0x00, 0x40, //std %r7, 0x40(%r1)
            0xF8, 0xE1, 0x00, 0x48, //std %r7, 0x48(%r1)
            0xF8, 0xC1, 0x00, 0x50, //std %r6, 0x50(%r1)
            0x38, 0x80, 0x00, 0x00, //li %r4, 0
            0x38, 0xA1, 0x00, 0x30, //addi %r5, %r1, 0x30
            0x39, 0x60, 0x01, 0xE1, //li %r11, 0x1E1
            0x44, 0x00, 0x00, 0x02, //;sc                   #SYS_PRX_START_MODULE(sys_prx_id_t id, sys_prx_flags_t flags, sys_prx_start_t* pOpt)
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x80, 0x01, 0x44, //blt 0x144
            0xF8, 0x41, 0x00, 0x18, //std %r2, 0x18(%r1)
            0xE8, 0x81, 0x00, 0x40, //ld %r4, 0x40(%r1)
            0x80, 0x04, 0x00, 0x00, //lwz %r0, 0(%r4)
            0x80, 0x44, 0x00, 0x04, //lwz %r2, 0x4(%r4)
            0x7C, 0x09, 0x03, 0xA6, //mtctr %r0
            0x4E, 0x80, 0x04, 0x21, //bctrl                 #PRX_ENTRY_FUNCTION(void)
            0xE8, 0x41, 0x00, 0x18, //ld %r2, 0x18(%r1)
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x80, 0x64, 0x00, 0x04, //lwz %r3, 0x4(%r4)
            0x38, 0x80, 0x00, 0x28, //li %r4, 0x28
            0x38, 0xA0, 0x00, 0x02, //li %r5, 0x2
            0x38, 0xE0, 0x00, 0x00, //li %r7, 0
            0x38, 0xC0, 0xFF, 0xFF, //li %r6, -1
            0xF8, 0x81, 0x00, 0x30, //std %r4, 0x30(%r1)
            0xF8, 0xA1, 0x00, 0x38, //std %r5, 0x38(%r1)
            0xF8, 0xE1, 0x00, 0x40, //std %r7, 0x40(%r1)
            0xF8, 0xE1, 0x00, 0x48, //std %r7, 0x48(%r1)
            0xF8, 0xC1, 0x00, 0x50, //std %r6, 0x50(%r1)
            0x38, 0x80, 0x00, 0x00, //li %r4, 0
            0x38, 0xA1, 0x00, 0x30, //addi %r5, %r1, 0x30
            0x39, 0x60, 0x01, 0xE1, //li %r11, 0x1E1
            0x44, 0x00, 0x00, 0x02, //;sc                   #SYS_PRX_START_MODULE(sys_prx_id_t id, sys_prx_flags_t flags, sys_prx_start_t* pOpt)
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x80, 0x00, 0xE4, //blt 0xE4
            0x48, 0x00, 0x00, 0xDC, //b 0xDC
            0x2C, 0x03, 0x00, 0x02, //cmpwi %r3, 2
            0x40, 0x82, 0x00, 0xE0, //bne 0xE0
            0x38, 0x60, 0x00, 0x00, //li %r3, 0x0
            0x90, 0x64, 0x00, 0x00, //stw %r3, 0(%r4)
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x80, 0x64, 0x00, 0x04, //lwz %r3, 0x4(%r4)
            0x38, 0x80, 0x00, 0x28, //li %r4, 0x28
            0x38, 0xA0, 0x00, 0x01, //li %r5, 0x1
            0x38, 0xE0, 0x00, 0x00, //li %r7, 0
            0x38, 0xC0, 0xFF, 0xFF, //li %r6, -1
            0xF8, 0x81, 0x00, 0x30, //std %r4, 0x30(%r1)
            0xF8, 0xA1, 0x00, 0x38, //std %r5, 0x38(%r1)
            0xF8, 0xE1, 0x00, 0x40, //std %r7, 0x40(%r1)
            0xF8, 0xE1, 0x00, 0x48, //std %r7, 0x48(%r1)
            0xF8, 0xC1, 0x00, 0x50, //std %r6, 0x50(%r1)
            0x38, 0x80, 0x00, 0x00, //li %r4, 0
            0x38, 0xA1, 0x00, 0x30, //addi %r5, %r1, 0x30
            0x39, 0x60, 0x01, 0xE2, //li %r11, 0x1E2
            0x44, 0x00, 0x00, 0x02, //;sc                   #SYS_PRX_STOP_MODULE(sys_prx_id_t id, sys_prx_flags_t flags, sys_prx_stop_t* pOpt)
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x80, 0x00, 0x8C, //blt 0x8C
            0xF8, 0x41, 0x00, 0x18, //std %r2, 0x18(%r1)
            0xE8, 0x81, 0x00, 0x40, //ld %r4, 0x40(%r1)
            0x80, 0x04, 0x00, 0x00, //lwz %r0, 0(%r4)
            0x80, 0x44, 0x00, 0x04, //lwz %r2, 0x4(%r4)
            0x7C, 0x09, 0x03, 0xA6, //mtctr %r0
            0x4E, 0x80, 0x04, 0x21, //bctrl                 #PRX_EXIT_FUNCTION(void)
            0xE8, 0x41, 0x00, 0x18, //ld %r2, 0x18(%r1)
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x80, 0x64, 0x00, 0x04, //lwz %r3, 0x4(%r4)
            0x38, 0x80, 0x00, 0x28, //li %r4, 0x28
            0x38, 0xA0, 0x00, 0x02, //li %r5, 0x2
            0x38, 0xE0, 0x00, 0x00, //li %r7, 0
            0x38, 0xC0, 0xFF, 0xFF, //li %r6, -1
            0xF8, 0x81, 0x00, 0x30, //std %r4, 0x30(%r1)
            0xF8, 0xA1, 0x00, 0x38, //std %r5, 0x38(%r1)
            0xF8, 0xE1, 0x00, 0x40, //std %r7, 0x40(%r1)
            0xF8, 0xE1, 0x00, 0x48, //std %r7, 0x48(%r1)
            0xF8, 0xC1, 0x00, 0x50, //std %r6, 0x50(%r1)
            0x38, 0x80, 0x00, 0x00, //li %r4, 0
            0x38, 0xA1, 0x00, 0x30, //addi %r5, %r1, 0x30
            0x39, 0x60, 0x01, 0xE2, //li %r11, 0x1E2
            0x44, 0x00, 0x00, 0x02, //;sc                   #SYS_PRX_STOP_MODULE(sys_prx_id_t id, sys_prx_flags_t flags, sys_prx_stop_t* pOpt)
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x80, 0x00, 0x2C, //blt 0x2C
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x80, 0x64, 0x00, 0x04, //lwz %r3, 0x4(%r4)
            0x38, 0xC0, 0x00, 0x00, //li %r6, 0
            0x38, 0x80, 0x00, 0x00, //li %r4, 0
            0x38, 0xA0, 0x00, 0x00, //li %r5, 0
            0x39, 0x60, 0x01, 0xE3, //li %r11, 0x1E3
            0x44, 0x00, 0x00, 0x02, //;sc                   #SYS_PRX_UNLOAD_MODULE(sys_prx_id_t id, sys_prx_flags_t flags, sys_prx_unload_module_option_t* pOpt)
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x80, 0x00, 0x08, //blt 0x8
            0x38, 0x60, 0x00, 0x00, //li %r3, 0x0
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x90, 0x64, 0x00, 0x08, //stw %r3, 0x8(%r4)
            0xE8, 0x01, 0x00, 0x80, //ld %r0, 0x80(%r1)
            0x7C, 0x08, 0x03, 0xA6, //mtlr %r0
            0x38, 0x21, 0x00, 0x70, //addi %r1, %r1, 0x70
            0x4E, 0x80, 0x00, 0x20  //blr
        };


        public RPC(TMAPI PS3) {
            RPC.PS3 = PS3;
        }

        public static bool Enable(string game) {
            if (!RPC_ENABLED) {
                RPC_INSTALL_ADDR = RPC_ADDRESS.GetAddress(game);
                if (RPC_INSTALL_ADDR != 0x0) {
                    PS3.GetMemory(RPC_INSTALL_ADDR, RESTORE_0); //save first instruction
                    PS3.GetMemory(RPC_INSTALL_ADDR + 0x4, RESTORE_BYTES); //save bytes being overwritten

                    PS3.Ext.WriteUInt32(ENABLE_ADDR, 0x0); //make sure rpc is not trying to execute

                    PS3.SetMemory(RPC_INSTALL_ADDR, RPC_BLR); //blr function to prevent freezes
                    PS3.SetMemory(RPC_INSTALL_ADDR + 0x4, RPC_BYTES); //install rpc
                    PS3.SetMemory(RPC_INSTALL_ADDR, RPC_ADJUST_STACK); //allow rpc execution

                    RPC_ENABLED = true;
                    return true;
                }
                return false;
            }
            return true;
        }

        public static void Disable() {
            if(RPC_ENABLED) {
                PS3.Ext.WriteUInt32(ENABLE_ADDR, 0x0); //make sure rpc is not trying to execute

                PS3.SetMemory(RPC_INSTALL_ADDR, RPC_BLR); //blr function to prevent freezes
                PS3.SetMemory(RPC_INSTALL_ADDR + 0x4, RESTORE_BYTES); //restore original bytes
                PS3.SetMemory(RPC_INSTALL_ADDR, RESTORE_0); //restore first instruction

                RPC_ENABLED = false;
            }
        }

        public static uint LoadModule(string path) {
            if(RPC_ENABLED) {
                PS3.Ext.WriteString(PLUGIN_PATH_ADDR, path);
                PS3.Ext.WriteUInt32(ENABLE_ADDR, 0x1);

                Thread.Sleep(100);

                return PS3.Ext.ReadUInt32(MODULE_ERROR_ADDR);
            }

            return 0x0;
        }

        public static uint UnloadModule(uint prxId) {
            if (RPC_ENABLED) {
                PS3.Ext.WriteUInt32(MODULE_IDS_ADDR, prxId);
                PS3.Ext.WriteUInt32(ENABLE_ADDR, 0x2);

                Thread.Sleep(100);

                return PS3.Ext.ReadUInt32(MODULE_ERROR_ADDR);
            }
            return 0x0;
        }

        public static uint[] GetModules() {
            return PS3.ModuleIds();
        }
    }

    public static class RPC_ADDRESS {
        public const uint COD4 = 0x0;
        public const uint WAW = 0x0;
        public const uint MW2 = 0x2539F8;
        public const uint BO1 = 0x432FF8;
        public const uint MW3 = 0xE1C58;
        public const uint BO2 = 0x3708D0;
        public const uint GHOST = 0xB143C;
        public const uint AW = 0xEA4E8;

        public static uint GetAddress(string game) {
            if (game.Contains("Black Ops")) {
                if (game.Contains("II"))
                    return BO2;
                return BO1;
            }
               
            if (game.Contains("Modern Warfare")) {
                if (game.Contains("3"))
                    return MW3;
                if (game.Contains("2"))
                    return MW2;
                return COD4;
            }

            if (game.Contains("Ghosts"))
                return GHOST;

            if (game.Contains("Advanced Warfare"))
                return AW;

            if (game.Contains("World at War"))
                return WAW;

            return 0x0;
        }
    }
}
