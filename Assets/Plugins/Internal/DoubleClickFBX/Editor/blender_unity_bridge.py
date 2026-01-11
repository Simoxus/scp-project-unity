import bpy
import os
bpy.context.preferences.view.show_splash = False

CLOSE_ON_SAVE = False

class UnityFbxExporter:
    def __init__(self, fbx_path):
        self.fbx_path = fbx_path
        self.filename = os.path.basename(fbx_path)
    
    def load_fbx(self):
        """Load the FBX file into Blender"""

        # Delete everything in startup scene
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.delete(use_global=False)

        bpy.ops.import_scene.fbx(filepath=self.fbx_path)
        bpy.context.scene['unity_fbx_path'] = self.fbx_path

        bpy.context.tool_settings.mesh_select_mode = (False, False, True)
        bpy.ops.object.select_all(action='SELECT')

        # Set shading to solid and also zoom in
        for area in bpy.context.screen.areas:
            if area.type == 'VIEW_3D':
                for space in area.spaces:
                    if space.type == 'VIEW_3D':
                        space.shading.type = 'SOLID'

                override = {'area': area, 'region': area.regions[-1]}
                with bpy.context.temp_override(**override):
                    bpy.ops.view3d.view_selected()

        bpy.ops.object.select_all(action='DESELECT')

        print(f"Loaded '{self.filename}'")
    
    @staticmethod
    def export_to_unity(filepath):
        """Export scene back to Unity"""

        bpy.ops.export_scene.fbx(
            filepath=filepath,
            global_scale=1.0,
            apply_unit_scale=True,
            apply_scale_options='FBX_SCALE_UNITS',
            object_types={'ARMATURE', 'MESH', 'EMPTY'},
            add_leaf_bones=False,
            primary_bone_axis='Y',
            secondary_bone_axis='X',
            armature_nodetype='NULL',
            bake_anim=True,
            axis_forward='-Z',
            axis_up='Y'
        )

class WM_OT_save_unity_fbx(bpy.types.Operator):
    bl_idname = "wm.save_unity_fbx"
    bl_label = "Save Unity FBX"
    bl_description = "Export to Unity FBX"
    bl_options = {'REGISTER'}
    
    def execute(self, context):
        if 'unity_fbx_path' not in context.scene:
            bpy.ops.wm.save_mainfile('INVOKE_DEFAULT')
            return {'FINISHED'}
        
        path = context.scene['unity_fbx_path']
        
        try:
            # Export to Unity
            UnityFbxExporter.export_to_unity(path)
            self.report({'INFO'}, f"Saved to Unity: {os.path.basename(path)}")

            if CLOSE_ON_SAVE:
                bpy.ops.wm.quit_blender()
            
        except Exception as e:
            self.report({'ERROR'}, f"Save failed {str(e)}")
            return {'CANCELLED'}
        
        return {'FINISHED'}

def menu_func_export(self, context):
    if 'unity_fbx_path' in context.scene:
        self.layout.operator(
            WM_OT_save_unity_fbx.bl_idname,
            text="Unity FBX (back to original)",
            icon='EXPORT'
        )

addon_keymaps = []

def register():
    bpy.utils.register_class(WM_OT_save_unity_fbx)
    bpy.types.TOPBAR_MT_file_export.append(menu_func_export)
    
    # Add keybind
    wm = bpy.context.window_manager
    kc = wm.keyconfigs.addon
    if kc:
        km = kc.keymaps.new(name='Window', space_type='EMPTY')
        kmi = km.keymap_items.new(WM_OT_save_unity_fbx.bl_idname, 'S', 'PRESS', ctrl=True)
        addon_keymaps.append((km, kmi))

def unregister():
    # Remove keybind
    for km, kmi in addon_keymaps:
        km.keymap_items.remove(kmi)
    addon_keymaps.clear()
    
    bpy.types.TOPBAR_MT_file_export.remove(menu_func_export)
    bpy.utils.unregister_class(WM_OT_save_unity_fbx)

if __name__ == "__main__":
    import sys
    if "--" in sys.argv:
        argv = sys.argv[sys.argv.index("--") + 1:]
        if argv:
            fbx_path = argv[0]
            exporter = UnityFbxExporter(fbx_path)
            exporter.load_fbx()
            register()