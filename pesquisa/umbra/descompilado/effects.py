
# ==== .LightEffectManager.__init__ (linha 1510)
self.sound_min = 0
self.sound_max = 0
self.sound_vaule_lists = []
self.width = width
self.height = height
self.color_buffer = np.zeros((height, width, 3), dtype=(np.uint8))
self.color_buffer.fill(0)
self.color_buffer_flag = 0
self.config_file = port.os.path.join(port.current_dir, f"C:\\Users\\{port.username}\\AppData\\Roaming\\robobloq\\hid_ble\\fan_mubu_effect_config.json")
self.default_effects = {'rainbow':{
   'name': "彩虹流动",
   'type': "rainbow",
   'speed': 4,
   'position': 0}, 
 'wave':{'name':"波浪灯效", 
  'type':"wave", 
  'speed':4, 
  'color':[
   255, 255, 255], 
  'width':100, 
  'position':0}, 
 'block':{'name':"积木灯效", 
  'type':"block", 
  'speed':4, 
  'color':[
   255, 255, 0], 
  'height':100, 
  'position':0, 
  'fixed_blocks':[],  'current_block':0}, 
 'four_directions':{'name':"四向平铺", 
  'type':"four_directions", 
  'speed':4, 
  'colors':[
   [
    255, 0, 0],
   [
    255, 0, 255],
   [
    0, 0, 255],
   [
    255, 255, 0]], 
  'current_color':0, 
  'progress':0, 
  'direction':0, 
  'completed':False}, 
 'corner_fill':{'name':"四角填充", 
  'type':"corner_fill", 
  'speed':4, 
  'colors':[
   [
    255, 0, 0],
   [
    255, 0, 255],
   [
    0, 0, 255],
   [
    255, 255, 0]], 
  'current_color':0, 
  'progress':0, 
  'direction':0, 
  'completed':False}, 
 'random_circle':{'name':"随机圆环", 
  'type':"random_circle", 
  'speed':2, 
  'center_x':300, 
  'center_y':300, 
  'max_radius':250, 
  'current_radius':0, 
  'current_alpha':255, 
  'state':0, 
  'color':[
   255, 0, 0], 
  'next_color':None, 
  'circle_count':0}, 
 'fourth_dimension':{
   'name': "四次元",
   'type': "fourth_dimension",
   'speed': 50,
   'time_counter': 0,
   'grid_size': 100}, 
 'bubble':{'name':"气泡", 
  'type':"bubble", 
  'speed':5, 
  'colors':[
   [
    255, 0, 0],
   [
    0, 255, 0]], 
  'max_radius':100, 
  'min_radius':10, 
  'max_count':10, 
  'bubbles':[]}, 
 'dna_helix':{'name':"DNA 螺旋", 
  'type':"dna_helix", 
  'speed':5, 
  'color':[
   0, 255, 0], 
  'position':0}, 
 'radar':{'name':"雷达扫描", 
  'type':"radar", 
  'speed':5, 
  'colors':[
   [
    0, 255, 42],
   [
    255, 255, 255],
   [
    255, 255, 255],
   [
    0, 23, 4]], 
  'enable_blips':True, 
  'enable_ripples':True, 
  'move_blips':True, 
  'blip_size':10, 
  'angle':0, 
  'blips':[],  'ripple_timing':1}, 
 'impact':{'name':"撞击效果", 
  'type':"impact", 
  'speed':5, 
  'ball_radius':75, 
  'impact_effect_radius':50, 
  'ball_position':[
   300, 300], 
  'ball_direction':[
   1, 1], 
  'ball_color':[
   255, 0, 0], 
  'impact_position':None, 
  'impact_color':None, 
  'impact_radius':0, 
  'impact_alpha':255}, 
 'gradient_push':{'name':"渐变推动", 
  'type':"gradient_push", 
  'speed':5, 
  'rainbow_position':0, 
  'band_position':-50, 
  'band_color':[
   255, 255, 255], 
  'band_active':False, 
  'band_width':50, 
  'rainbow_width':255}, 
 'music_visualizer':{
   'name': "音乐可视化1",
   'type': "music_visualizer",
   'speed': 5,
   'sensitivity': 50,
   'position': 0,
   'volume': 0,
   'last_volume': 0,
   'color_band': None}, 
 'diagonal_visualizer':{'name':"音乐可视化2", 
  'type':"diagonal_visualizer", 
  'color':[
   0, 255, 0], 
  'speed':5, 
  'last_volume':0}, 
 'music_center_side':{'name':"音乐3", 
  'type':"music_center_side", 
  'speed':5, 
  'color':[
   0, 255, 255], 
  'intensity':1.0, 
  'fade_factor':5, 
  'last_volume':0}, 
 'sound_bar_visualizer':{'name':"音乐4", 
  'type':"sound_bar_visualizer", 
  'speed':5, 
  'volume':0, 
  'bars':[
   300] * 10, 
  'color_position':0}, 
 'screen_effects':{
   'name': "同屏",
   'type': "screen_effects",
   'speed': 5,
   'quse': 0}, 
 'snake':{'name':"贪吃蛇", 
  'type':"snake", 
  'speed':5, 
  'snake_body':[],  'snake_direction':[
   1, 0], 
  'food_position':None, 
  'food_color':None, 
  'snake_color':[
   0, 255, 0], 
  'body_height':40, 
  'body_length':2000, 
  'segment_length':40, 
  'food_radius':50, 
  'path':[]}, 
 'clock':{'name':"时钟", 
  'type':"clock", 
  'speed':5, 
  'colors':[
   [
    255, 0, 0],
   [
    0, 255, 0],
   [
    0, 0, 255],
   [
    0, 0, 0]], 
  'hour_hand_width':60, 
  'minute_hand_width':45, 
  'second_hand_width':30}, 
 'fireworks':{'name':"烟花", 
  'type':"fireworks", 
  'colors':[
   [
    255, 255, 0],
   [
    255, 0, 0]], 
  'speed':5, 
  'explode_speed_min':2, 
  'explode_speed_max':10, 
  'particle_count':30, 
  'particle_size':5, 
  'fireworks':[]}, 
 'matrix_flow':{'name':"矩阵流动", 
  'type':"matrix_flow", 
  'speed':5, 
  'position':0, 
  'matrix_width':200, 
  'matrix_length':1200, 
  'spacing':400, 
  'colors':[
   [
    255, 0, 0],
   [
    0, 0, 0]]}, 
 'matrix_rain':{'name':"黑客帝国", 
  'type':"matrix_rain", 
  'speed':5, 
  'color':[
   0, 255, 34], 
  'font_size':60, 
  'chars':"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 
  'drops':[],  'last_update':(time.time)()}, 
 'fluid_fill':{'name':"流体填充", 
  'type':"fluid_fill", 
  'speed':5, 
  'color':[
   0, 0, 255], 
  'progress':0, 
  'stage':0, 
  'direction':0, 
  'wave_height':30, 
  'wave_speed':2, 
  'wave_offset':0}, 
 'box_pushing':{'name':"推箱子堆叠", 
  'type':"box_pushing", 
  'speed':5, 
  'color':[
   0, 0, 255], 
  'box_size':100, 
  'grid':None, 
  'active_box':None, 
  'state':0, 
  'fade_progress':0, 
  'current_row':0, 
  'current_col':0}, 
 'black_hole':{'name':"黑洞", 
  'type':"black_hole", 
  'speed':5, 
  'colors':[
   [
    255, 0, 0],
   [
    0, 255, 255],
   [
    255, 255, 0],
   [
    255, 0, 255]]}, 
 'custom_wave':{'name':"定制波浪", 
  'type':"custom_wave", 
  'speed':5, 
  'n_colors':4, 
  'vertical':False, 
  'colors':[
   [
    255, 0, 0],
   [
    0, 0, 255],
   [
    0, 255, 0],
   [
    255, 255, 0]]}, 
 'fire':{'name':"火焰", 
  'type':"fire", 
  'speed':5, 
  'colors':[
   [
    20, 10, 5],
   [
    60, 20, 5],
   [
    180, 80, 0],
   [
    240, 160, 40]]}, 
 'aurora':{'name':"极光", 
  'type':"aurora", 
  'speed':5}, 
 'riptide_wave':{'name':"激流", 
  'type':"riptide_wave", 
  'speed':30, 
  'colors':[
   [
    0, 26, 51],
   [
    6, 66, 115],
   [
    255, 255, 255]]}, 
 'gradient':{'name':"线性渐变", 
  'type':"gradient", 
  'speed':5, 
  'numColors':4, 
  'colors':[
   [
    0, 239, 255],
   [
    255, 0, 207],
   [
    204, 255, 0],
   [
    0, 255, 95]], 
  'positions':[
   0, 121, 248, 320]}, 
 'interleave':{'name':"交错", 
  'type':"interleave", 
  'speed':5, 
  'colors':[
   [
    17, 150, 0],
   [
    255, 255, 0],
   [
    255, 0, 255],
   [
    0, 0, 255]]}, 
 'spiral_rainbow':{
   'name': "螺旋彩虹",
   'type': "spiral_rainbow",
   'speed': 5,
   'xPos': 160,
   'yPos': 100,
   'reversed': 0}, 
 'meteor':{'name':"流星", 
  'type':"meteor", 
  'speed':5, 
  'size':10, 
  'path':1, 
  'colors':[
   [
    231, 67, 204],
   [
    153, 67, 231],
   [
    255, 246, 101],
   [
    0, 0, 0]]}, 
 'sky_cloud':{'name':"蓝天白云", 
  'type':"sky_cloud", 
  'timeOfDay':2, 
  'direction':0, 
  'speed':5, 
  'daySpeed':15, 
  'size':0, 
  'colors':[
   [
    0, 87, 201],
   [
    240, 249, 255],
   [
    0, 14, 106],
   [
    142, 155, 237]]}, 
 'camo_fade':{'name':"迷彩褪色", 
  'type':"camo_fade", 
  'speed':5, 
  'colors':[
   [
    158, 96, 35],
   [
    158, 33, 86],
   [
    92, 65, 120],
   [
    0, 62, 128]]}, 
 'lasers_lasers':{'name':"激光", 
  'type':"lasers_lasers", 
  'speed':5, 
  'colors':[
   [
    255, 8, 1],
   [
    0, 84, 249]]}, 
 'storm':{'name':"暴风雨", 
  'type':"storm", 
  'speed':5, 
  'colors':[
   [
    0, 20, 40],
   [
    0, 250, 255],
   [
    0, 140, 255]]}, 
 'explode':{'name':"爆炸", 
  'type':"explode", 
  'speed':5, 
  'colors':[
   [
    0, 7, 26],
   [
    0, 200, 255]]}, 
 'rainbow_tunnel':{'name':"彩虹隧道", 
  'type':"rainbow_tunnel", 
  'speed':5}, 
 'saiyan':{'name':"超级赛亚人", 
  'type':"saiyan", 
  'speed':5, 
  'colors':[
   [
    48, 46, 0],
   [
    214, 255, 0],
   [
    255, 255, 255]]}, 
 'spiral':{'name':"定制螺旋", 
  'type':"spiral", 
  'speed':5, 
  'colors':[
   [
    255, 0, 0],
   [
    0, 0, 255],
   [
    0, 255, 0],
   [
    255, 255, 0]]}, 
 'poison':{'name':"毒药", 
  'type':"poison", 
  'speed':5, 
  'colors':[
   [
    19, 0, 50],
   [
    96, 0, 252],
   [
    179, 0, 255],
   [
    138, 66, 255]]}, 
 'windmill':{'name':"风车", 
  'type':"windmill", 
  'speed':5, 
  'colors':[
   [
    255, 185, 0],
   [
    163, 0, 240],
   [
    0, 0, 0],
   [
    0, 0, 0]]}, 
 'maple_leaf':{'name':"枫叶", 
  'type':"maple_leaf", 
  'speed':5, 
  'colors':[
   [
    40, 17, 0],
   [
    255, 173, 22],
   [
    255, 105, 5],
   [
    255, 0, 0]]}, 
 'lightsaber_battle':{'name':"光剑决斗", 
  'type':"lightsaber_battle", 
  'speed':5, 
  'colors':[
   [
    255, 0, 0],
   [
    0, 0, 255],
   [
    0, 0, 0],
   [
    0, 0, 0]]}, 
 'trace_growth':{'name':"轨迹生长", 
  'type':"trace_growth", 
  'speed':5, 
  'colors':[
   [
    66, 135, 245],
   [
    0, 0, 0],
   [
    0, 0, 0],
   [
    0, 0, 0]]}, 
 'police_light':{'name':"警灯", 
  'type':"police_light", 
  'speed':5, 
  'colors':[
   [
    255, 0, 0],
   [
    255, 255, 255],
   [
    0, 0, 255],
   [
    0, 0, 0]]}, 
 'sphere_3d':{'name':"3D球面光环", 
  'type':"sphere_3d", 
  'speed':5, 
  'colors':[
   [
    255, 0, 0],
   [
    255, 255, 0],
   [
    0, 255, 255],
   [
    255, 0, 255]]}, 
 'dream_dream':{'name':"迷幻梦境", 
  'type':"dream_dream", 
  'speed':5, 
  'colors':[
   [
    36, 0, 75],
   [
    255, 0, 255],
   [
    0, 255, 255],
   [
    139, 0, 255]]}, 
 'neon_wave':{'name':"霓虹日落波浪", 
  'type':"neon_wave", 
  'speed':5, 
  'colors':[
   [
    255, 0, 255],
   [
    255, 0, 129],
   [
    0, 117, 255],
   [
    0, 255, 255]]}, 
 'fireflies':{'name':"萤火虫", 
  'type':"fireflies", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    50, 255, 50],
   [
    255, 255, 0],
   [
    255, 100, 0]]}, 
 'hellfire':{'name':"地狱之火", 
  'type':"hellfire", 
  'speed':5, 
  'colors':[
   [
    1, 1, 1],
   [
    255, 25, 6],
   [
    255, 80, 0],
   [
    255, 160, 0]]}, 
 'dragon_flame':{'name':"龙之焰", 
  'type':"dragon_flame", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    255, 0, 0],
   [
    255, 120, 0],
   [
    255, 200, 0]]}, 
 'hydrogen':{'name':"氢原子轨道", 
  'type':"hydrogen", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    63, 0, 0],
   [
    255, 178, 0],
   [
    242, 255, 198]]}, 
 'hyperspace':{'name':"超空间星场", 
  'type':"hyperspace", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    255, 255, 255],
   [
    0, 201, 252],
   [
    255, 255, 0]]}, 
 'jellyfish':{'name':"飘逸水母", 
  'type':"jellyfish", 
  'speed':5, 
  'colors':[
   [
    4, 0, 20],
   [
    30, 180, 220],
   [
    80, 220, 255],
   [
    140, 240, 255]]}, 
 'koi_pond':{'name':"锦鲤池塘", 
  'type':"koi_pond", 
  'speed':5, 
  'colors':[
   [
    15, 48, 88],
   [
    252, 165, 0],
   [
    255, 255, 255],
   [
    37, 109, 15]]}, 
 'lasers':{'name':"激光舞台", 
  'type':"lasers", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    255, 8, 1],
   [
    0, 84, 249],
   [
    35, 255, 49]]}, 
 'lava_lamp':{'name':"熔岩灯", 
  'type':"lava_lamp", 
  'speed':5, 
  'colors':[
   [
    11, 3, 18],
   [
    0, 13, 237],
   [
    228, 0, 32],
   [
    0, 0, 0]]}, 
 'mario':{'name':"超级马里奥", 
  'type':"mario", 
  'speed':5, 
  'colors':[
   [
    13, 27, 51],
   [
    255, 69, 0],
   [
    255, 215, 0],
   [
    34, 139, 34]]}, 
 'multiverse_simple':{'name':"多元宇宙", 
  'type':"multiverse_simple", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    0, 255, 0]]}, 
 'cosmic_nebula':{'name':"星云", 
  'type':"cosmic_nebula", 
  'speed':5, 
  'colors':[
   [
    0, 6, 32],
   [
    80, 180, 255],
   [
    220, 100, 255],
   [
    255, 255, 255]]}, 
 'neon_grid':{'name':"霓虹网格", 
  'type':"neon_grid", 
  'speed':5, 
  'colors':[
   [
    34, 0, 29],
   [
    204, 0, 204],
   [
    255, 0, 255],
   [
    255, 255, 255]]}, 
 'neurolink':{'name':"神经链接", 
  'type':"neurolink", 
  'speed':5, 
  'colors':[
   [
    0, 0, 25],
   [
    0, 255, 0],
   [
    0, 255, 255]]}, 
 'new_years':{'name':"新年烟花", 
  'type':"new_years", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    255, 50, 50],
   [
    255, 255, 0],
   [
    255, 255, 255]]}, 
 'paint_splatters':{'name':"颜料泼溅", 
  'type':"paint_splatters", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    255, 0, 255],
   [
    255, 255, 0],
   [
    0, 255, 255]]}, 
 'party_parrot':{'name':"派对鹦鹉", 
  'type':"party_parrot", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    255, 60, 0],
   [
    0, 255, 255],
   [
    255, 255, 0]]}, 
 'pixel_wave':{'name':"像素波浪", 
  'type':"pixel_wave", 
  'speed':5, 
  'colors':[
   [
    21, 26, 44],
   [
    255, 0, 157],
   [
    0, 195, 199],
   [
    0, 131, 199]]}, 
 'planetary_orbit':{'name':"行星轨道", 
  'type':"planetary_orbit", 
  'speed':5, 
  'colors':[
   [
    0, 16, 36],
   [
    255, 200, 0],
   [
    0, 255, 255],
   [
    255, 255, 255]]}, 
 'plasma_flow':{'name':"等离子流动", 
  'type':"plasma_flow", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    0, 220, 255],
   [
    255, 0, 160],
   [
    255, 255, 0]]}, 
 'poison_max':{'name':"剧毒弥漫", 
  'type':"poison_max", 
  'speed':5, 
  'colors':[
   [
    19, 0, 50],
   [
    96, 0, 252],
   [
    179, 0, 255],
   [
    138, 66, 255]]}, 
 'rainbow_tap':{'name':"彩虹波纹", 
  'type':"rainbow_tap", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    255, 0, 255],
   [
    0, 255, 255],
   [
    255, 255, 0]]}, 
 'resident_evil':{'name':"生化危机", 
  'type':"resident_evil", 
  'speed':5, 
  'colors':[
   [
    0, 0, 0],
   [
    255, 29, 29],
   [
    239, 222, 204]]}, 
 'rick_morty':{'name':"瑞克和莫蒂", 
  'type':"rick_morty", 
  'speed':5, 
  'colors':[
   [
    5, 0, 51],
   [
    78, 255, 0],
   [
    255, 230, 100],
   [
    255, 255, 255]]}}
self.load_config()
# ==== .LightEffectManager.load_config (linha 2341)
import json_init, json
if not port.os.path.exists(self.config_file):
    with open((self.config_file), "w", encoding="utf-8") as f:
        json.dump((json_init.MUBU_EFFECT_INIT), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {self.config_file}")
else:
    print(f"文件 {self.config_file} 已存在，不会写入新数据。")
    try:
        with open((self.config_file), "r", encoding="utf-8") as file:
            data = json.load(file)
            if data["ver"] < json_init.MUBU_EFFECT_INIT["ver"]:
                with open((self.config_file), "w", encoding="utf-8") as f:
                    json.dump((json_init.MUBU_EFFECT_INIT), f, ensure_ascii=False, indent=4)
                print(f"JSON 数据已成功写入到 {self.config_file}")
    except:
        print("打开产品配置文件出错")
        with open((self.config_file), "w", encoding="utf-8") as f:
            json.dump((json_init.MUBU_EFFECT_INIT), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {self.config_file}")

if port.FAN_EFFECT_VER:
    with open((self.config_file), "w", encoding="utf-8") as f:
        json.dump((json_init.MUBU_EFFECT_INIT), f, ensure_ascii=False, indent=4)
    print("开发阶段，每一次打开软件都会清除幕布灯效配置")
try:
    with open((self.config_file), "r", encoding="utf-8") as f:
        config = json.load(f)
        self.current_effect = config.get("current_effect")
        self.ver_effect = config.get("ver")
        self.effect_brightness = config.get("brightness")
        self.effects = {}
        for effect_name, effect_config in self.default_effects.items():
            effect_config = self.default_effects[effect_name].copy()
            if effect_name in config["effects"]:
                for key, value in config["effects"][effect_name].items():
                    if key in effect_config:
                        effect_config[key] = value
                    self.effects[effect_name] = effect_config

except (FileNotFoundError, json.JSONDecodeError) as e:
    try:
        print(f"加载配置文件失败，使用默认配置: {str(e)}")
        self.current_effect = "rainbow"
        self.effects = self.default_effects.copy()
    finally:
        e = None
        del e
# ==== .LightEffectManager.save_config (linha 2403)
try:
    config = {'ver':self.ver_effect, 
     'current_effect':self.current_effect, 
     'brightness':self.effect_brightness, 
     'effects':{}}
    runtime_keys = {
     "position","progress","completed",
     "current_color", "direction", "current_block",
     "impact_color", "band_position", "band_color", "bars",
     "drops"}
    for effect_name, effect_config in self.effects.items():
        config["effects"][effect_name] = {k: v for k, v in effect_config.items() if k not in runtime_keys}

    with open((self.config_file), "w", encoding="utf-8") as f:
        print(f"幕布灯效信息已保存到：{self.config_file}")
        json.dump(config, f, indent=4, ensure_ascii=False)
except Exception as e:
    try:
        print(f"保存灯效配置失败: {str(e)}")
        QMessageBox.warning(None, "保存失败", f"无法保存配置: {str(e)}")
    finally:
        e = None
        del e
# ==== .LightEffectManager.save_config.<dictcomp> (linha 2420)
{k: v for (k, v) in .0 if k not in runtime_keys}
# ==== .LightEffectManager.update_effect (linha 2434)
# descompilação falhou (demorou demais); desmontagem:
# # pydisasm version 6.1.7
# # Python bytecode 3.8.0 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       update_effect
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  3
# # Stack size:        4
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        2434
# # Constants:
# #    0: None
# #    1: (0, 0)
# #    2: 1
# #    3: 'type'
# #    4: 'screen_effects'
# #    5: 0
# #    6: 'rainbow'
# #    7: 'wave'
# #    8: 'block'
# #    9: 'four_directions'
# #   10: 'corner_fill'
# #   11: 'random_circle'
# #   12: 'fourth_dimension'
# #   13: 'bubble'
# #   14: 'dna_helix'
# #   15: 'radar'
# #   16: 'impact'
# #   17: 'gradient_push'
# #   18: 'music_visualizer'
# #   19: 'diagonal_visualizer'
# #   20: 'music_center_side'
# #   21: 'snake'
# #   22: 'clock'
# #   23: 'fireworks'
# #   24: 'sound_bar_visualizer'
# #   25: 'matrix_flow'
# #   26: 'matrix_rain'
# #   27: 'fluid_fill'
# #   28: 'box_pushing'
# #   29: 'lightsaber_duel'
# #   30: 'black_hole'
# #   31: 'custom_wave'
# #   32: 'fire'
# #   33: 'aurora'
# #   34: 'riptide_wave'
# #   35: 'gradient'
# #   36: 'interleave'
# #   37: 'spiral_rainbow'
# #   38: 'meteor'
# #   39: 'sky_cloud'
# #   40: 'camo_fade'
# #   41: 'lasers_lasers'
# #   42: 'storm'
# #   43: 'explode'
# #   44: 'rainbow_tunnel'
# #   45: 'saiyan'
# #   46: 'spiral'
# #   47: 'poison'
# #   48: 'windmill'
# #   49: 'maple_leaf'
# #   50: 'lightsaber_battle'
# #   51: 'trace_growth'
# #   52: 'police_light'
# #   53: 'sphere_3d'
# #   54: 'math_3d_rose'
# #   55: 'dream_dream'
# #   56: 'neon_wave'
# #   57: 'fireflies'
# #   58: 'hellfire'
# #   59: 'dragon_flame'
# #   60: 'hydrogen'
# #   61: 'hyperspace'
# #   62: 'jellyfish'
# #   63: 'koi_pond'
# #   64: 'lasers'
# #   65: 'lava_lamp'
# #   66: 'mario'
# #   67: 'multiverse_simple'
# #   68: 'cosmic_nebula'
# #   69: 'neon_grid'
# #   70: 'neurolink'
# #   71: 'new_years'
# #   72: 'paint_splatters'
# #   73: 'party_parrot'
# #   74: 'pixel_wave'
# #   75: 'planetary_orbit'
# #   76: 'plasma_flow'
# #   77: 'poison_max'
# #   78: 'rainbow_tap'
# #   79: 'resident_evil'
# #   80: 'rick_morty'
# # Names:
# #    0: effects
# #    1: current_effect
# #    2: read_vaule
# #    3: changer_vaule
# #    4: _update_rainbow_effect
# #    5: _update_wave_effect
# #    6: _update_block_effect
# #    7: _update_four_directions_effect
# #    8: _update_corner_fill_effect
# #    9: _update_random_circle_effect
# #   10: _update_fourth_dimension_effect
# #   11: _update_bubble_effect
# #   12: _update_dna_helix_links_effect
# #   13: _update_radar_effect
# #   14: _update_impact_effect
# #   15: _update_gradient_push_effect
# #   16: _update_music_visualizer_effect
# #   17: _update_diagonal_visualizer_effect
# #   18: _update_music_center_side_effect
# #   19: _update_tongping_effect
# #   20: _update_snake_effect
# #   21: _update_clock_effect
# #   22: _update_fireworks_effect
# #   23: _update_sound_bar_visualizer_effect
# #   24: _update_matrix_flow_effect
# #   25: _update_matrix_rain_effect
# #   26: _update_fluid_fill_effect
# #   27: _update_box_pushing_effect
# #   28: _update_lightsaber_duel_effect
# #   29: _update_black_hole_effect
# #   30: _update_custom_wave_effect
# #   31: _update_fire_effect
# #   32: _update_aurora_effect
# #   33: _update_riptide_wave_effect
# #   34: _update_gradient_effect
# #   35: _update_interleave_effect
# #   36: _update_spiral_rainbow_effect
# #   37: _update_meteor_effect
# #   38: _update_sky_cloud_effect
# #   39: _update_camo_fade_effect
# #   40: _update_lasers_lasers_effect
# #   41: _update_storm_effect
# #   42: _update_explode_effect
# #   43: _update_rainbow_tunnel_effect
# #   44: _update_saiyan_effect
# #   45: _update_spiral_effect
# #   46: _update_poison_effect
# #   47: _update_windmill_effect
# #   48: _update_maple_leaf_effect
# #   49: _update_lightsaber_battle_effect
# #   50: _update_trace_growth_effect
# #   51: _update_police_light_effect
# #   52: _update_sphere_3d_effect
# #   53: _update_math_3d_super_shape_effect
# #   54: _update_dream_dream_effect
# #   55: _update_neon_wave_effect
# #   56: _update_fireflies_effect
# #   57: _update_hellfire_effect
# #   58: _update_dragon_flame_effect
# #   59: _update_hydrogen_effect
# #   60: _update_hyperspace_effect
# #   61: _update_jellyfish_effect
# #   62: _update_koi_pond_effect
# #   63: _update_lasers_effect
# #   64: _update_lava_lamp_effect
# #   65: _update_mario_effect
# #   66: _update_multiverse_simple_effect
# #   67: render_cosmic_nebula
# #   68: render_neon_grid
# #   69: render_neurolink
# #   70: render_new_years
# #   71: render_paint_splatters
# #   72: render_party_parrot
# #   73: render_pixel_wave
# #   74: render_planetary_orbit
# #   75: render_plasma_flow
# #   76: render_poison
# #   77: render_rainbow_tap
# #   78: render_resident_evil
# #   79: render_rick_morty
# # Varnames:
# #	self, effect, read_vaule_temp
# # Positional arguments:
# #	self
# # Local variables:
# #    1: effect
# #    2: read_vaule_temp
# 
# # Method Name:       update_effect
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  3
# # Stack size:        4
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        2434
# # Constants:
# #    0: None
# #    1: (0, 0)
# #    2: 1
# #    3: 'type'
# #    4: 'screen_effects'
# #    5: 0
# #    6: 'rainbow'
# #    7: 'wave'
# #    8: 'block'
# #    9: 'four_directions'
# #   10: 'corner_fill'
# #   11: 'random_circle'
# #   12: 'fourth_dimension'
# #   13: 'bubble'
# #   14: 'dna_helix'
# #   15: 'radar'
# #   16: 'impact'
# #   17: 'gradient_push'
# #   18: 'music_visualizer'
# #   19: 'diagonal_visualizer'
# #   20: 'music_center_side'
# #   21: 'snake'
# #   22: 'clock'
# #   23: 'fireworks'
# #   24: 'sound_bar_visualizer'
# #   25: 'matrix_flow'
# #   26: 'matrix_rain'
# #   27: 'fluid_fill'
# #   28: 'box_pushing'
# #   29: 'lightsaber_duel'
# #   30: 'black_hole'
# #   31: 'custom_wave'
# #   32: 'fire'
# #   33: 'aurora'
# #   34: 'riptide_wave'
# #   35: 'gradient'
# #   36: 'interleave'
# #   37: 'spiral_rainbow'
# #   38: 'meteor'
# #   39: 'sky_cloud'
# #   40: 'camo_fade'
# #   41: 'lasers_lasers'
# #   42: 'storm'
# #   43: 'explode'
# #   44: 'rainbow_tunnel'
# #   45: 'saiyan'
# #   46: 'spiral'
# #   47: 'poison'
# #   48: 'windmill'
# #   49: 'maple_leaf'
# #   50: 'lightsaber_battle'
# #   51: 'trace_growth'
# #   52: 'police_light'
# #   53: 'sphere_3d'
# #   54: 'math_3d_rose'
# #   55: 'dream_dream'
# #   56: 'neon_wave'
# #   57: 'fireflies'
# #   58: 'hellfire'
# #   59: 'dragon_flame'
# #   60: 'hydrogen'
# #   61: 'hyperspace'
# #   62: 'jellyfish'
# #   63: 'koi_pond'
# #   64: 'lasers'
# #   65: 'lava_lamp'
# #   66: 'mario'
# #   67: 'multiverse_simple'
# #   68: 'cosmic_nebula'
# #   69: 'neon_grid'
# #   70: 'neurolink'
# #   71: 'new_years'
# #   72: 'paint_splatters'
# #   73: 'party_parrot'
# #   74: 'pixel_wave'
# #   75: 'planetary_orbit'
# #   76: 'plasma_flow'
# #   77: 'poison_max'
# #   78: 'rainbow_tap'
# #   79: 'resident_evil'
# #   80: 'rick_morty'
# # Names:
# #    0: effects
# #    1: current_effect
# #    2: read_vaule
# #    3: changer_vaule
# #    4: _update_rainbow_effect
# #    5: _update_wave_effect
# #    6: _update_block_effect
# #    7: _update_four_directions_effect
# #    8: _update_corner_fill_effect
# #    9: _update_random_circle_effect
# #   10: _update_fourth_dimension_effect
# #   11: _update_bubble_effect
# #   12: _update_dna_helix_links_effect
# #   13: _update_radar_effect
# #   14: _update_impact_effect
# #   15: _update_gradient_push_effect
# #   16: _update_music_visualizer_effect
# #   17: _update_diagonal_visualizer_effect
# #   18: _update_music_center_side_effect
# #   19: _update_tongping_effect
# #   20: _update_snake_effect
# #   21: _update_clock_effect
# #   22: _update_fireworks_effect
# #   23: _update_sound_bar_visualizer_effect
# #   24: _update_matrix_flow_effect
# #   25: _update_matrix_rain_effect
# #   26: _update_fluid_fill_effect
# #   27: _update_box_pushing_effect
# #   28: _update_lightsaber_duel_effect
# #   29: _update_black_hole_effect
# #   30: _update_custom_wave_effect
# #   31: _update_fire_effect
# #   32: _update_aurora_effect
# #   33: _update_riptide_wave_effect
# #   34: _update_gradient_effect
# #   35: _update_interleave_effect
# #   36: _update_spiral_rainbow_effect
# #   37: _update_meteor_effect
# #   38: _update_sky_cloud_effect
# #   39: _update_camo_fade_effect
# #   40: _update_lasers_lasers_effect
# #   41: _update_storm_effect
# #   42: _update_explode_effect
# #   43: _update_rainbow_tunnel_effect
# #   44: _update_saiyan_effect
# #   45: _update_spiral_effect
# #   46: _update_poison_effect
# #   47: _update_windmill_effect
# #   48: _update_maple_leaf_effect
# #   49: _update_lightsaber_battle_effect
# #   50: _update_trace_growth_effect
# #   51: _update_police_light_effect
# #   52: _update_sphere_3d_effect
# #   53: _update_math_3d_super_shape_effect
# #   54: _update_dream_dream_effect
# #   55: _update_neon_wave_effect
# #   56: _update_fireflies_effect
# #   57: _update_hellfire_effect
# #   58: _update_dragon_flame_effect
# #   59: _update_hydrogen_effect
# #   60: _update_hyperspace_effect
# #   61: _update_jellyfish_effect
# #   62: _update_koi_pond_effect
# #   63: _update_lasers_effect
# #   64: _update_lava_lamp_effect
# #   65: _update_mario_effect
# #   66: _update_multiverse_simple_effect
# #   67: render_cosmic_nebula
# #   68: render_neon_grid
# #   69: render_neurolink
# #   70: render_new_years
# #   71: render_paint_splatters
# #   72: render_party_parrot
# #   73: render_pixel_wave
# #   74: render_planetary_orbit
# #   75: render_plasma_flow
# #   76: render_poison
# #   77: render_rainbow_tap
# #   78: render_resident_evil
# #   79: render_rick_morty
# # Varnames:
# #	self, effect, read_vaule_temp
# # Positional arguments:
# #	self
# # Local variables:
# #    1: effect
# #    2: read_vaule_temp
# 2436:           0 LOAD_FAST            (self)
#                2 LOAD_ATTR            (effects)
#                4 LOAD_FAST            (self)
#                6 LOAD_ATTR            (current_effect)
#                8 BINARY_SUBSCR
#               10 STORE_FAST           (effect)
# 
# 2437:          12 LOAD_GLOBAL          (read_vaule)
#               14 CALL_FUNCTION        (0 positional arguments)
#               16 STORE_FAST           (read_vaule_temp)
# 
# 2438:          18 LOAD_FAST            (read_vaule_temp)
#               20 LOAD_CONST           ((0, 0))
#               22 BINARY_SUBSCR
#               24 LOAD_CONST           (1)
#               26 COMPARE_OP           (==)
#               28 POP_JUMP_IF_FALSE    (to 54)
#               30 LOAD_FAST            (effect)
#               32 LOAD_CONST           ("type")
#               34 BINARY_SUBSCR
#               36 LOAD_CONST           ("screen_effects")
#               38 COMPARE_OP           (!=)
#               40 POP_JUMP_IF_FALSE    (to 54)
# 
# 2439:          42 LOAD_GLOBAL          (changer_vaule)
#               44 LOAD_CONST           (0)
#               46 LOAD_CONST           (0)
#               48 LOAD_CONST           (0)
#               50 CALL_FUNCTION        (3 positional arguments)
#               52 POP_TOP
# 
# 2440:     >>   54 LOAD_FAST            (effect)
#               56 LOAD_CONST           ("type")
#               58 BINARY_SUBSCR
#               60 LOAD_CONST           ("rainbow")
#               62 COMPARE_OP           (==)
#               64 POP_JUMP_IF_FALSE    (to 78)
# 
# 2441:          66 LOAD_FAST            (self)
#               68 LOAD_METHOD          (_update_rainbow_effect)
#               70 CALL_METHOD          (0 positional arguments)
#               72 POP_TOP
#               74 EXTENDED_ARG         (1792)
#               76 JUMP_FORWARD         (to 1990)
# 
# 2442:     >>   78 LOAD_FAST            (effect)
#               80 LOAD_CONST           ("type")
#               82 BINARY_SUBSCR
#               84 LOAD_CONST           ("wave")
#               86 COMPARE_OP           (==)
#               88 POP_JUMP_IF_FALSE    (to 102)
# 
# 2443:          90 LOAD_FAST            (self)
#               92 LOAD_METHOD          (_update_wave_effect)
#               94 CALL_METHOD          (0 positional arguments)
#               96 POP_TOP
#               98 EXTENDED_ARG         (1792)
#              100 JUMP_FORWARD         (to 1990)
# 
# 2444:     >>  102 LOAD_FAST            (effect)
#              104 LOAD_CONST           ("type")
#              106 BINARY_SUBSCR
#              108 LOAD_CONST           ("block")
#              110 COMPARE_OP           (==)
#              112 POP_JUMP_IF_FALSE    (to 126)
# 
# 2445:         114 LOAD_FAST            (self)
#              116 LOAD_METHOD          (_update_block_effect)
#              118 CALL_METHOD          (0 positional arguments)
#              120 POP_TOP
#              122 EXTENDED_ARG         (1792)
#              124 JUMP_FORWARD         (to 1990)
# 
# 2446:     >>  126 LOAD_FAST            (effect)
#              128 LOAD_CONST           ("type")
#              130 BINARY_SUBSCR
#              132 LOAD_CONST           ("four_directions")
#              134 COMPARE_OP           (==)
#              136 POP_JUMP_IF_FALSE    (to 150)
# 
# 2447:         138 LOAD_FAST            (self)
#              140 LOAD_METHOD          (_update_four_directions_effect)
#              142 CALL_METHOD          (0 positional arguments)
#              144 POP_TOP
#              146 EXTENDED_ARG         (1792)
#              148 JUMP_FORWARD         (to 1990)
# 
# 2448:     >>  150 LOAD_FAST            (effect)
#              152 LOAD_CONST           ("type")
#              154 BINARY_SUBSCR
#              156 LOAD_CONST           ("corner_fill")
#              158 COMPARE_OP           (==)
#              160 POP_JUMP_IF_FALSE    (to 174)
# 
# 2449:         162 LOAD_FAST            (self)
#              164 LOAD_METHOD          (_update_corner_fill_effect)
#              166 CALL_METHOD          (0 positional arguments)
#              168 POP_TOP
#              170 EXTENDED_ARG         (1792)
#              172 JUMP_FORWARD         (to 1990)
# 
# 2450:     >>  174 LOAD_FAST            (effect)
#              176 LOAD_CONST           ("type")
#              178 BINARY_SUBSCR
#              180 LOAD_CONST           ("random_circle")
#              182 COMPARE_OP           (==)
#              184 POP_JUMP_IF_FALSE    (to 198)
# 
# 2451:         186 LOAD_FAST            (self)
#              188 LOAD_METHOD          (_update_random_circle_effect)
#              190 CALL_METHOD          (0 positional arguments)
#              192 POP_TOP
#              194 EXTENDED_ARG         (1792)
#              196 JUMP_FORWARD         (to 1990)
# 
# 2452:     >>  198 LOAD_FAST            (effect)
#              200 LOAD_CONST           ("type")
#              202 BINARY_SUBSCR
#              204 LOAD_CONST           ("fourth_dimension")
#              206 COMPARE_OP           (==)
#              208 POP_JUMP_IF_FALSE    (to 222)
# 
# 2453:         210 LOAD_FAST            (self)
#              212 LOAD_METHOD          (_update_fourth_dimension_effect)
#              214 CALL_METHOD          (0 positional arguments)
#              216 POP_TOP
#              218 EXTENDED_ARG         (1536)
#              220 JUMP_FORWARD         (to 1990)
# 
# 2454:     >>  222 LOAD_FAST            (effect)
#              224 LOAD_CONST           ("type")
#              226 BINARY_SUBSCR
#              228 LOAD_CONST           ("bubble")
#              230 COMPARE_OP           (==)
#              232 POP_JUMP_IF_FALSE    (to 246)
# 
# 2455:         234 LOAD_FAST            (self)
#              236 LOAD_METHOD          (_update_bubble_effect)
#              238 CALL_METHOD          (0 positional arguments)
#              240 POP_TOP
#              242 EXTENDED_ARG         (1536)
#              244 JUMP_FORWARD         (to 1990)
# 
# 2456:     >>  246 LOAD_FAST            (effect)
#              248 LOAD_CONST           ("type")
#              250 BINARY_SUBSCR
#              252 LOAD_CONST           ("dna_helix")
#              254 COMPARE_OP           (==)
#              256 EXTENDED_ARG         (256)
#              258 POP_JUMP_IF_FALSE    (to 272)
# 
# 2457:         260 LOAD_FAST            (self)
#              262 LOAD_METHOD          (_update_dna_helix_links_effect)
#              264 CALL_METHOD          (0 positional arguments)
#              266 POP_TOP
#              268 EXTENDED_ARG         (1536)
#              270 JUMP_FORWARD         (to 1990)
# 
# 2458:     >>  272 LOAD_FAST            (effect)
#              274 LOAD_CONST           ("type")
#              276 BINARY_SUBSCR
#              278 LOAD_CONST           ("radar")
#              280 COMPARE_OP           (==)
#              282 EXTENDED_ARG         (256)
#              284 POP_JUMP_IF_FALSE    (to 298)
# 
# 2459:         286 LOAD_FAST            (self)
#              288 LOAD_METHOD          (_update_radar_effect)
#              290 CALL_METHOD          (0 positional arguments)
#              292 POP_TOP
#              294 EXTENDED_ARG         (1536)
#              296 JUMP_FORWARD         (to 1990)
# 
# 2460:     >>  298 LOAD_FAST            (effect)
#              300 LOAD_CONST           ("type")
#              302 BINARY_SUBSCR
#              304 LOAD_CONST           ("impact")
#              306 COMPARE_OP           (==)
#              308 EXTENDED_ARG         (256)
#              310 POP_JUMP_IF_FALSE    (to 324)
# 
# 2461:         312 LOAD_FAST            (self)
#              314 LOAD_METHOD          (_update_impact_effect)
#              316 CALL_METHOD          (0 positional arguments)
#              318 POP_TOP
#              320 EXTENDED_ARG         (1536)
#              322 JUMP_FORWARD         (to 1990)
# 
# 2462:     >>  324 LOAD_FAST            (effect)
#              326 LOAD_CONST           ("type")
#              328 BINARY_SUBSCR
#              330 LOAD_CONST           ("gradient_push")
#              332 COMPARE_OP           (==)
#              334 EXTENDED_ARG         (256)
#              336 POP_JUMP_IF_FALSE    (to 350)
# 
# 2463:         338 LOAD_FAST            (self)
#              340 LOAD_METHOD          (_update_gradient_push_effect)
#              342 CALL_METHOD          (0 positional arguments)
#              344 POP_TOP
#              346 EXTENDED_ARG         (1536)
#              348 JUMP_FORWARD         (to 1990)
# 
# 2464:     >>  350 LOAD_FAST            (effect)
#              352 LOAD_CONST           ("type")
#              354 BINARY_SUBSCR
#              356 LOAD_CONST           ("music_visualizer")
#              358 COMPARE_OP           (==)
#              360 EXTENDED_ARG         (256)
#              362 POP_JUMP_IF_FALSE    (to 376)
# 
# 2465:         364 LOAD_FAST            (self)
#              366 LOAD_METHOD          (_update_music_visualizer_effect)
#              368 CALL_METHOD          (0 positional arguments)
#              370 POP_TOP
#              372 EXTENDED_ARG         (1536)
#              374 JUMP_FORWARD         (to 1990)
# 
# 2466:     >>  376 LOAD_FAST            (effect)
#              378 LOAD_CONST           ("type")
#              380 BINARY_SUBSCR
#              382 LOAD_CONST           ("diagonal_visualizer")
#              384 COMPARE_OP           (==)
#              386 EXTENDED_ARG         (256)
#              388 POP_JUMP_IF_FALSE    (to 402)
# 
# 2467:         390 LOAD_FAST            (self)
#              392 LOAD_METHOD          (_update_diagonal_visualizer_effect)
#              394 CALL_METHOD          (0 positional arguments)
#              396 POP_TOP
#              398 EXTENDED_ARG         (1536)
#              400 JUMP_FORWARD         (to 1990)
# 
# 2468:     >>  402 LOAD_FAST            (effect)
#              404 LOAD_CONST           ("type")
#              406 BINARY_SUBSCR
#              408 LOAD_CONST           ("music_center_side")
#              410 COMPARE_OP           (==)
#              412 EXTENDED_ARG         (256)
#              414 POP_JUMP_IF_FALSE    (to 428)
# 
# 2469:         416 LOAD_FAST            (self)
#              418 LOAD_METHOD          (_update_music_center_side_effect)
#              420 CALL_METHOD          (0 positional arguments)
#              422 POP_TOP
#              424 EXTENDED_ARG         (1536)
#              426 JUMP_FORWARD         (to 1990)
# 
# 2470:     >>  428 LOAD_FAST            (effect)
#              430 LOAD_CONST           ("type")
#              432 BINARY_SUBSCR
#              434 LOAD_CONST           ("screen_effects")
#              436 COMPARE_OP           (==)
#              438 EXTENDED_ARG         (256)
#              440 POP_JUMP_IF_FALSE    (to 454)
# 
# 2471:         442 LOAD_FAST            (self)
#              444 LOAD_METHOD          (_update_tongping_effect)
#              446 CALL_METHOD          (0 positional arguments)
#              448 POP_TOP
#              450 EXTENDED_ARG         (1536)
#              452 JUMP_FORWARD         (to 1990)
# 
# 2472:     >>  454 LOAD_FAST            (effect)
#              456 LOAD_CONST           ("type")
#              458 BINARY_SUBSCR
#              460 LOAD_CONST           ("snake")
#              462 COMPARE_OP           (==)
#              464 EXTENDED_ARG         (256)
#              466 POP_JUMP_IF_FALSE    (to 480)
# 
# 2473:         468 LOAD_FAST            (self)
#              470 LOAD_METHOD          (_update_snake_effect)
#              472 CALL_METHOD          (0 positional arguments)
#              474 POP_TOP
#              476 EXTENDED_ARG         (1280)
#              478 JUMP_FORWARD         (to 1990)
# 
# 2474:     >>  480 LOAD_FAST            (effect)
#              482 LOAD_CONST           ("type")
#              484 BINARY_SUBSCR
#              486 LOAD_CONST           ("clock")
#              488 COMPARE_OP           (==)
#              490 EXTENDED_ARG         (256)
#              492 POP_JUMP_IF_FALSE    (to 506)
# 
# 2475:         494 LOAD_FAST            (self)
#              496 LOAD_METHOD          (_update_clock_effect)
#              498 CALL_METHOD          (0 positional arguments)
#              500 POP_TOP
#              502 EXTENDED_ARG         (1280)
#              504 JUMP_FORWARD         (to 1990)
# 
# 2476:     >>  506 LOAD_FAST            (effect)
#              508 LOAD_CONST           ("type")
#              510 BINARY_SUBSCR
#              512 LOAD_CONST           ("fireworks")
#              514 COMPARE_OP           (==)
#              516 EXTENDED_ARG         (512)
#              518 POP_JUMP_IF_FALSE    (to 532)
# 
# 2477:         520 LOAD_FAST            (self)
#              522 LOAD_METHOD          (_update_fireworks_effect)
#              524 CALL_METHOD          (0 positional arguments)
#              526 POP_TOP
#              528 EXTENDED_ARG         (1280)
#              530 JUMP_FORWARD         (to 1990)
# 
# 2478:     >>  532 LOAD_FAST            (effect)
#              534 LOAD_CONST           ("type")
#              536 BINARY_SUBSCR
#              538 LOAD_CONST           ("sound_bar_visualizer")
#              540 COMPARE_OP           (==)
#              542 EXTENDED_ARG         (512)
#              544 POP_JUMP_IF_FALSE    (to 558)
# 
# 2479:         546 LOAD_FAST            (self)
#              548 LOAD_METHOD          (_update_sound_bar_visualizer_effect)
#              550 CALL_METHOD          (0 positional arguments)
#              552 POP_TOP
#              554 EXTENDED_ARG         (1280)
#              556 JUMP_FORWARD         (to 1990)
# 
# 2480:     >>  558 LOAD_FAST            (effect)
#              560 LOAD_CONST           ("type")
#              562 BINARY_SUBSCR
#              564 LOAD_CONST           ("matrix_flow")
#              566 COMPARE_OP           (==)
#              568 EXTENDED_ARG         (512)
#              570 POP_JUMP_IF_FALSE    (to 584)
# 
# 2481:         572 LOAD_FAST            (self)
#              574 LOAD_METHOD          (_update_matrix_flow_effect)
#              576 CALL_METHOD          (0 positional arguments)
#              578 POP_TOP
#              580 EXTENDED_ARG         (1280)
#              582 JUMP_FORWARD         (to 1990)
# 
# 2482:     >>  584 LOAD_FAST            (effect)
#              586 LOAD_CONST           ("type")
#              588 BINARY_SUBSCR
#              590 LOAD_CONST           ("matrix_rain")
#              592 COMPARE_OP           (==)
#              594 EXTENDED_ARG         (512)
#              596 POP_JUMP_IF_FALSE    (to 610)
# 
# 2483:         598 LOAD_FAST            (self)
#              600 LOAD_METHOD          (_update_matrix_rain_effect)
#              602 CALL_METHOD          (0 positional arguments)
#              604 POP_TOP
#              606 EXTENDED_ARG         (1280)
#              608 JUMP_FORWARD         (to 1990)
# 
# 2484:     >>  610 LOAD_FAST            (effect)
#              612 LOAD_CONST           ("type")
#              614 BINARY_SUBSCR
#              616 LOAD_CONST           ("fluid_fill")
#              618 COMPARE_OP           (==)
#              620 EXTENDED_ARG         (512)
#              622 POP_JUMP_IF_FALSE    (to 636)
# 
# 2485:         624 LOAD_FAST            (self)
#              626 LOAD_METHOD          (_update_fluid_fill_effect)
#              628 CALL_METHOD          (0 positional arguments)
#              630 POP_TOP
#              632 EXTENDED_ARG         (1280)
#              634 JUMP_FORWARD         (to 1990)
# 
# 2486:     >>  636 LOAD_FAST            (effect)
#              638 LOAD_CONST           ("type")
#              640 BINARY_SUBSCR
#              642 LOAD_CONST           ("box_pushing")
#              644 COMPARE_OP           (==)
#              646 EXTENDED_ARG         (512)
#              648 POP_JUMP_IF_FALSE    (to 662)
# 
# 2487:         650 LOAD_FAST            (self)
#              652 LOAD_METHOD          (_update_box_pushing_effect)
#              654 CALL_METHOD          (0 positional arguments)
#              656 POP_TOP
#              658 EXTENDED_ARG         (1280)
#              660 JUMP_FORWARD         (to 1990)
# 
# 2488:     >>  662 LOAD_FAST            (effect)
#              664 LOAD_CONST           ("type")
#              666 BINARY_SUBSCR
#              668 LOAD_CONST           ("lightsaber_duel")
#              670 COMPARE_OP           (==)
#              672 EXTENDED_ARG         (512)
#              674 POP_JUMP_IF_FALSE    (to 688)
# 
# 2489:         676 LOAD_FAST            (self)
#              678 LOAD_METHOD          (_update_lightsaber_duel_effect)
#              680 CALL_METHOD          (0 positional arguments)
#              682 POP_TOP
#              684 EXTENDED_ARG         (1280)
#              686 JUMP_FORWARD         (to 1990)
# 
# 2490:     >>  688 LOAD_FAST            (effect)
#              690 LOAD_CONST           ("type")
#              692 BINARY_SUBSCR
#              694 LOAD_CONST           ("black_hole")
#              696 COMPARE_OP           (==)
#              698 EXTENDED_ARG         (512)
#              700 POP_JUMP_IF_FALSE    (to 714)
# 
# 2491:         702 LOAD_FAST            (self)
#              704 LOAD_METHOD          (_update_black_hole_effect)
#              706 CALL_METHOD          (0 positional arguments)
#              708 POP_TOP
#              710 EXTENDED_ARG         (1024)
#              712 JUMP_FORWARD         (to 1990)
# 
# 2492:     >>  714 LOAD_FAST            (effect)
#              716 LOAD_CONST           ("type")
#              718 BINARY_SUBSCR
#              720 LOAD_CONST           ("custom_wave")
#              722 COMPARE_OP           (==)
#              724 EXTENDED_ARG         (512)
#              726 POP_JUMP_IF_FALSE    (to 740)
# 
# 2493:         728 LOAD_FAST            (self)
#              730 LOAD_METHOD          (_update_custom_wave_effect)
#              732 CALL_METHOD          (0 positional arguments)
#              734 POP_TOP
#              736 EXTENDED_ARG         (1024)
#              738 JUMP_FORWARD         (to 1990)
# 
# 2494:     >>  740 LOAD_FAST            (effect)
#              742 LOAD_CONST           ("type")
#              744 BINARY_SUBSCR
#              746 LOAD_CONST           ("fire")
#              748 COMPARE_OP           (==)
#              750 EXTENDED_ARG         (512)
#              752 POP_JUMP_IF_FALSE    (to 766)
# 
# 2495:         754 LOAD_FAST            (self)
#              756 LOAD_METHOD          (_update_fire_effect)
#              758 CALL_METHOD          (0 positional arguments)
#              760 POP_TOP
#              762 EXTENDED_ARG         (1024)
#              764 JUMP_FORWARD         (to 1990)
# 
# 2496:     >>  766 LOAD_FAST            (effect)
#              768 LOAD_CONST           ("type")
#              770 BINARY_SUBSCR
#              772 LOAD_CONST           ("aurora")
#              774 COMPARE_OP           (==)
#              776 EXTENDED_ARG         (768)
#              778 POP_JUMP_IF_FALSE    (to 792)
# 
# 2497:         780 LOAD_FAST            (self)
#              782 LOAD_METHOD          (_update_aurora_effect)
#              784 CALL_METHOD          (0 positional arguments)
#              786 POP_TOP
#              788 EXTENDED_ARG         (1024)
#              790 JUMP_FORWARD         (to 1990)
# 
# 2498:     >>  792 LOAD_FAST            (effect)
#              794 LOAD_CONST           ("type")
#              796 BINARY_SUBSCR
#              798 LOAD_CONST           ("riptide_wave")
#              800 COMPARE_OP           (==)
#              802 EXTENDED_ARG         (768)
#              804 POP_JUMP_IF_FALSE    (to 818)
# 
# 2499:         806 LOAD_FAST            (self)
#              808 LOAD_METHOD          (_update_riptide_wave_effect)
#              810 CALL_METHOD          (0 positional arguments)
#              812 POP_TOP
#              814 EXTENDED_ARG         (1024)
#              816 JUMP_FORWARD         (to 1990)
# 
# 2500:     >>  818 LOAD_FAST            (effect)
#              820 LOAD_CONST           ("type")
#              822 BINARY_SUBSCR
#              824 LOAD_CONST           ("gradient")
#              826 COMPARE_OP           (==)
#              828 EXTENDED_ARG         (768)
#              830 POP_JUMP_IF_FALSE    (to 844)
# 
# 2501:         832 LOAD_FAST            (self)
#              834 LOAD_METHOD          (_update_gradient_effect)
#              836 CALL_METHOD          (0 positional arguments)
#              838 POP_TOP
#              840 EXTENDED_ARG         (1024)
#              842 JUMP_FORWARD         (to 1990)
# 
# 2502:     >>  844 LOAD_FAST            (effect)
#              846 LOAD_CONST           ("type")
#              848 BINARY_SUBSCR
#              850 LOAD_CONST           ("interleave")
#              852 COMPARE_OP           (==)
#              854 EXTENDED_ARG         (768)
#              856 POP_JUMP_IF_FALSE    (to 870)
# 
# 2503:         858 LOAD_FAST            (self)
#              860 LOAD_METHOD          (_update_interleave_effect)
#              862 CALL_METHOD          (0 positional arguments)
#              864 POP_TOP
#              866 EXTENDED_ARG         (1024)
#              868 JUMP_FORWARD         (to 1990)
# 
# 2504:     >>  870 LOAD_FAST            (effect)
#              872 LOAD_CONST           ("type")
#              874 BINARY_SUBSCR
#              876 LOAD_CONST           ("spiral_rainbow")
#              878 COMPARE_OP           (==)
#              880 EXTENDED_ARG         (768)
#              882 POP_JUMP_IF_FALSE    (to 896)
# 
# 2505:         884 LOAD_FAST            (self)
#              886 LOAD_METHOD          (_update_spiral_rainbow_effect)
#              888 CALL_METHOD          (0 positional arguments)
#              890 POP_TOP
#              892 EXTENDED_ARG         (1024)
#              894 JUMP_FORWARD         (to 1990)
# 
# 2506:     >>  896 LOAD_FAST            (effect)
#              898 LOAD_CONST           ("type")
#              900 BINARY_SUBSCR
#              902 LOAD_CONST           ("meteor")
#              904 COMPARE_OP           (==)
#              906 EXTENDED_ARG         (768)
#              908 POP_JUMP_IF_FALSE    (to 922)
# 
# 2507:         910 LOAD_FAST            (self)
#              912 LOAD_METHOD          (_update_meteor_effect)
#              914 CALL_METHOD          (0 positional arguments)
#              916 POP_TOP
#              918 EXTENDED_ARG         (1024)
#              920 JUMP_FORWARD         (to 1990)
# 
# 2508:     >>  922 LOAD_FAST            (effect)
#              924 LOAD_CONST           ("type")
#              926 BINARY_SUBSCR
#              928 LOAD_CONST           ("sky_cloud")
#              930 COMPARE_OP           (==)
#              932 EXTENDED_ARG         (768)
#              934 POP_JUMP_IF_FALSE    (to 948)
# 
# 2509:         936 LOAD_FAST            (self)
#              938 LOAD_METHOD          (_update_sky_cloud_effect)
#              940 CALL_METHOD          (0 positional arguments)
#              942 POP_TOP
#              944 EXTENDED_ARG         (1024)
#              946 JUMP_FORWARD         (to 1990)
# 
# 2510:     >>  948 LOAD_FAST            (effect)
#              950 LOAD_CONST           ("type")
#              952 BINARY_SUBSCR
#              954 LOAD_CONST           ("camo_fade")
#              956 COMPARE_OP           (==)
#              958 EXTENDED_ARG         (768)
#              960 POP_JUMP_IF_FALSE    (to 974)
# 
# 2511:         962 LOAD_FAST            (self)
#              964 LOAD_METHOD          (_update_camo_fade_effect)
#              966 CALL_METHOD          (0 positional arguments)
#              968 POP_TOP
#              970 EXTENDED_ARG         (768)
#              972 JUMP_FORWARD         (to 1990)
# 
# 2512:     >>  974 LOAD_FAST            (effect)
#              976 LOAD_CONST           ("type")
#              978 BINARY_SUBSCR
#              980 LOAD_CONST           ("lasers_lasers")
#              982 COMPARE_OP           (==)
#              984 EXTENDED_ARG         (768)
#              986 POP_JUMP_IF_FALSE    (to 1000)
# 
# 2513:         988 LOAD_FAST            (self)
#              990 LOAD_METHOD          (_update_lasers_lasers_effect)
#              992 CALL_METHOD          (0 positional arguments)
#              994 POP_TOP
#              996 EXTENDED_ARG         (768)
#              998 JUMP_FORWARD         (to 1990)
# 
# 2514:     >> 1000 LOAD_FAST            (effect)
#             1002 LOAD_CONST           ("type")
#             1004 BINARY_SUBSCR
#             1006 LOAD_CONST           ("storm")
#             1008 COMPARE_OP           (==)
#             1010 EXTENDED_ARG         (1024)
#             1012 POP_JUMP_IF_FALSE    (to 1026)
# 
# 2515:        1014 LOAD_FAST            (self)
#             1016 LOAD_METHOD          (_update_storm_effect)
#             1018 CALL_METHOD          (0 positional arguments)
#             1020 POP_TOP
#             1022 EXTENDED_ARG         (768)
#             1024 JUMP_FORWARD         (to 1990)
# 
# 2516:     >> 1026 LOAD_FAST            (effect)
#             1028 LOAD_CONST           ("type")
#             1030 BINARY_SUBSCR
#             1032 LOAD_CONST           ("explode")
#             1034 COMPARE_OP           (==)
#             1036 EXTENDED_ARG         (1024)
#             1038 POP_JUMP_IF_FALSE    (to 1052)
# 
# 2517:        1040 LOAD_FAST            (self)
#             1042 LOAD_METHOD          (_update_explode_effect)
#             1044 CALL_METHOD          (0 positional arguments)
#             1046 POP_TOP
#             1048 EXTENDED_ARG         (768)
#             1050 JUMP_FORWARD         (to 1990)
# 
# 2518:     >> 1052 LOAD_FAST            (effect)
#             1054 LOAD_CONST           ("type")
#             1056 BINARY_SUBSCR
#             1058 LOAD_CONST           ("rainbow_tunnel")
#             1060 COMPARE_OP           (==)
#             1062 EXTENDED_ARG         (1024)
#             1064 POP_JUMP_IF_FALSE    (to 1078)
# 
# 2519:        1066 LOAD_FAST            (self)
#             1068 LOAD_METHOD          (_update_rainbow_tunnel_effect)
#             1070 CALL_METHOD          (0 positional arguments)
#             1072 POP_TOP
#             1074 EXTENDED_ARG         (768)
#             1076 JUMP_FORWARD         (to 1990)
# 
# 2520:     >> 1078 LOAD_FAST            (effect)
#             1080 LOAD_CONST           ("type")
#             1082 BINARY_SUBSCR
#             1084 LOAD_CONST           ("saiyan")
#             1086 COMPARE_OP           (==)
#             1088 EXTENDED_ARG         (1024)
#             1090 POP_JUMP_IF_FALSE    (to 1104)
# 
# 2521:        1092 LOAD_FAST            (self)
#             1094 LOAD_METHOD          (_update_saiyan_effect)
#             1096 CALL_METHOD          (0 positional arguments)
#             1098 POP_TOP
#             1100 EXTENDED_ARG         (768)
#             1102 JUMP_FORWARD         (to 1990)
# 
# 2522:     >> 1104 LOAD_FAST            (effect)
#             1106 LOAD_CONST           ("type")
#             1108 BINARY_SUBSCR
#             1110 LOAD_CONST           ("spiral")
#             1112 COMPARE_OP           (==)
#             1114 EXTENDED_ARG         (1024)
#             1116 POP_JUMP_IF_FALSE    (to 1130)
# 
# 2523:        1118 LOAD_FAST            (self)
#             1120 LOAD_METHOD          (_update_spiral_effect)
#             1122 CALL_METHOD          (0 positional arguments)
#             1124 POP_TOP
#             1126 EXTENDED_ARG         (768)
#             1128 JUMP_FORWARD         (to 1990)
# 
# 2524:     >> 1130 LOAD_FAST            (effect)
#             1132 LOAD_CONST           ("type")
#             1134 BINARY_SUBSCR
#             1136 LOAD_CONST           ("poison")
#             1138 COMPARE_OP           (==)
#             1140 EXTENDED_ARG         (1024)
#             1142 POP_JUMP_IF_FALSE    (to 1156)
# 
# 2525:        1144 LOAD_FAST            (self)
#             1146 LOAD_METHOD          (_update_poison_effect)
#             1148 CALL_METHOD          (0 positional arguments)
#             1150 POP_TOP
#             1152 EXTENDED_ARG         (768)
#             1154 JUMP_FORWARD         (to 1990)
# 
# 2526:     >> 1156 LOAD_FAST            (effect)
#             1158 LOAD_CONST           ("type")
#             1160 BINARY_SUBSCR
#             1162 LOAD_CONST           ("windmill")
#             1164 COMPARE_OP           (==)
#             1166 EXTENDED_ARG         (1024)
#             1168 POP_JUMP_IF_FALSE    (to 1182)
# 
# 2527:        1170 LOAD_FAST            (self)
#             1172 LOAD_METHOD          (_update_windmill_effect)
#             1174 CALL_METHOD          (0 positional arguments)
#             1176 POP_TOP
#             1178 EXTENDED_ARG         (768)
#             1180 JUMP_FORWARD         (to 1990)
# 
# 2528:     >> 1182 LOAD_FAST            (effect)
#             1184 LOAD_CONST           ("type")
#             1186 BINARY_SUBSCR
#             1188 LOAD_CONST           ("maple_leaf")
#             1190 COMPARE_OP           (==)
#             1192 EXTENDED_ARG         (1024)
#             1194 POP_JUMP_IF_FALSE    (to 1208)
# 
# 2529:        1196 LOAD_FAST            (self)
#             1198 LOAD_METHOD          (_update_maple_leaf_effect)
#             1200 CALL_METHOD          (0 positional arguments)
#             1202 POP_TOP
#             1204 EXTENDED_ARG         (768)
#             1206 JUMP_FORWARD         (to 1990)
# 
# 2530:     >> 1208 LOAD_FAST            (effect)
#             1210 LOAD_CONST           ("type")
#             1212 BINARY_SUBSCR
#             1214 LOAD_CONST           ("lightsaber_battle")
#             1216 COMPARE_OP           (==)
#             1218 EXTENDED_ARG         (1024)
#             1220 POP_JUMP_IF_FALSE    (to 1234)
# 
# 2531:        1222 LOAD_FAST            (self)
#             1224 LOAD_METHOD          (_update_lightsaber_battle_effect)
#             1226 CALL_METHOD          (0 positional arguments)
#             1228 POP_TOP
#             1230 EXTENDED_ARG         (512)
#             1232 JUMP_FORWARD         (to 1990)
# 
# 2532:     >> 1234 LOAD_FAST            (effect)
#             1236 LOAD_CONST           ("type")
#             1238 BINARY_SUBSCR
#             1240 LOAD_CONST           ("trace_growth")
#             1242 COMPARE_OP           (==)
#             1244 EXTENDED_ARG         (1024)
#             1246 POP_JUMP_IF_FALSE    (to 1260)
# 
# 2533:        1248 LOAD_FAST            (self)
#             1250 LOAD_METHOD          (_update_trace_growth_effect)
#             1252 CALL_METHOD          (0 positional arguments)
#             1254 POP_TOP
#             1256 EXTENDED_ARG         (512)
#             1258 JUMP_FORWARD         (to 1990)
# 
# 2534:     >> 1260 LOAD_FAST            (effect)
#             1262 LOAD_CONST           ("type")
#             1264 BINARY_SUBSCR
#             1266 LOAD_CONST           ("police_light")
#             1268 COMPARE_OP           (==)
#             1270 EXTENDED_ARG         (1280)
#             1272 POP_JUMP_IF_FALSE    (to 1286)
# 
# 2535:        1274 LOAD_FAST            (self)
#             1276 LOAD_METHOD          (_update_police_light_effect)
#             1278 CALL_METHOD          (0 positional arguments)
#             1280 POP_TOP
#             1282 EXTENDED_ARG         (512)
#             1284 JUMP_FORWARD         (to 1990)
# 
# 2536:     >> 1286 LOAD_FAST            (effect)
#             1288 LOAD_CONST           ("type")
#             1290 BINARY_SUBSCR
#             1292 LOAD_CONST           ("sphere_3d")
#             1294 COMPARE_OP           (==)
#             1296 EXTENDED_ARG         (1280)
#             1298 POP_JUMP_IF_FALSE    (to 1312)
# 
# 2537:        1300 LOAD_FAST            (self)
#             1302 LOAD_METHOD          (_update_sphere_3d_effect)
#             1304 CALL_METHOD          (0 positional arguments)
#             1306 POP_TOP
#             1308 EXTENDED_ARG         (512)
#             1310 JUMP_FORWARD         (to 1990)
# 
# 2538:     >> 1312 LOAD_FAST            (effect)
#             1314 LOAD_CONST           ("type")
#             1316 BINARY_SUBSCR
#             1318 LOAD_CONST           ("math_3d_rose")
#             1320 COMPARE_OP           (==)
#             1322 EXTENDED_ARG         (1280)
#             1324 POP_JUMP_IF_FALSE    (to 1338)
# 
# 2539:        1326 LOAD_FAST            (self)
#             1328 LOAD_METHOD          (_update_math_3d_super_shape_effect)
#             1330 CALL_METHOD          (0 positional arguments)
#             1332 POP_TOP
#             1334 EXTENDED_ARG         (512)
#             1336 JUMP_FORWARD         (to 1990)
# 
# 2540:     >> 1338 LOAD_FAST            (effect)
#             1340 LOAD_CONST           ("type")
#             1342 BINARY_SUBSCR
#             1344 LOAD_CONST           ("dream_dream")
#             1346 COMPARE_OP           (==)
#             1348 EXTENDED_ARG         (1280)
#             1350 POP_JUMP_IF_FALSE    (to 1364)
# 
# 2541:        1352 LOAD_FAST            (self)
#             1354 LOAD_METHOD          (_update_dream_dream_effect)
#             1356 CALL_METHOD          (0 positional arguments)
#             1358 POP_TOP
#             1360 EXTENDED_ARG         (512)
#             1362 JUMP_FORWARD         (to 1990)
# 
# 2542:     >> 1364 LOAD_FAST            (effect)
#             1366 LOAD_CONST           ("type")
#             1368 BINARY_SUBSCR
#             1370 LOAD_CONST           ("neon_wave")
#             1372 COMPARE_OP           (==)
#             1374 EXTENDED_ARG         (1280)
#             1376 POP_JUMP_IF_FALSE    (to 1390)
# 
# 2543:        1378 LOAD_FAST            (self)
#             1380 LOAD_METHOD          (_update_neon_wave_effect)
#             1382 CALL_METHOD          (0 positional arguments)
#             1384 POP_TOP
#             1386 EXTENDED_ARG         (512)
#             1388 JUMP_FORWARD         (to 1990)
# 
# 2544:     >> 1390 LOAD_FAST            (effect)
#             1392 LOAD_CONST           ("type")
#             1394 BINARY_SUBSCR
#             1396 LOAD_CONST           ("fireflies")
#             1398 COMPARE_OP           (==)
#             1400 EXTENDED_ARG         (1280)
#             1402 POP_JUMP_IF_FALSE    (to 1416)
# 
# 2545:        1404 LOAD_FAST            (self)
#             1406 LOAD_METHOD          (_update_fireflies_effect)
#             1408 CALL_METHOD          (0 positional arguments)
#             1410 POP_TOP
#             1412 EXTENDED_ARG         (512)
#             1414 JUMP_FORWARD         (to 1990)
# 
# 2546:     >> 1416 LOAD_FAST            (effect)
#             1418 LOAD_CONST           ("type")
#             1420 BINARY_SUBSCR
#             1422 LOAD_CONST           ("hellfire")
#             1424 COMPARE_OP           (==)
#             1426 EXTENDED_ARG         (1280)
#             1428 POP_JUMP_IF_FALSE    (to 1442)
# 
# 2547:        1430 LOAD_FAST            (self)
#             1432 LOAD_METHOD          (_update_hellfire_effect)
#             1434 CALL_METHOD          (0 positional arguments)
#             1436 POP_TOP
#             1438 EXTENDED_ARG         (512)
#             1440 JUMP_FORWARD         (to 1990)
# 
# 2548:     >> 1442 LOAD_FAST            (effect)
#             1444 LOAD_CONST           ("type")
#             1446 BINARY_SUBSCR
#             1448 LOAD_CONST           ("dragon_flame")
#             1450 COMPARE_OP           (==)
#             1452 EXTENDED_ARG         (1280)
#             1454 POP_JUMP_IF_FALSE    (to 1468)
# 
# 2549:        1456 LOAD_FAST            (self)
#             1458 LOAD_METHOD          (_update_dragon_flame_effect)
#             1460 CALL_METHOD          (0 positional arguments)
#             1462 POP_TOP
#             1464 EXTENDED_ARG         (512)
#             1466 JUMP_FORWARD         (to 1990)
# 
# 2550:     >> 1468 LOAD_FAST            (effect)
#             1470 LOAD_CONST           ("type")
#             1472 BINARY_SUBSCR
#             1474 LOAD_CONST           ("hydrogen")
#             1476 COMPARE_OP           (==)
#             1478 EXTENDED_ARG         (1280)
#             1480 POP_JUMP_IF_FALSE    (to 1494)
# 
# 2551:        1482 LOAD_FAST            (self)
#             1484 LOAD_METHOD          (_update_hydrogen_effect)
#             1486 CALL_METHOD          (0 positional arguments)
#             1488 POP_TOP
#             1490 EXTENDED_ARG         (256)
#             1492 JUMP_FORWARD         (to 1990)
# 
# 2552:     >> 1494 LOAD_FAST            (effect)
#             1496 LOAD_CONST           ("type")
#             1498 BINARY_SUBSCR
#             1500 LOAD_CONST           ("hyperspace")
#             1502 COMPARE_OP           (==)
#             1504 EXTENDED_ARG         (1280)
#             1506 POP_JUMP_IF_FALSE    (to 1520)
# 
# 2553:        1508 LOAD_FAST            (self)
#             1510 LOAD_METHOD          (_update_hyperspace_effect)
#             1512 CALL_METHOD          (0 positional arguments)
#             1514 POP_TOP
#             1516 EXTENDED_ARG         (256)
#             1518 JUMP_FORWARD         (to 1990)
# 
# 2554:     >> 1520 LOAD_FAST            (effect)
#             1522 LOAD_CONST           ("type")
#             1524 BINARY_SUBSCR
#             1526 LOAD_CONST           ("jellyfish")
#             1528 COMPARE_OP           (==)
#             1530 EXTENDED_ARG         (1536)
#             1532 POP_JUMP_IF_FALSE    (to 1546)
# 
# 2555:        1534 LOAD_FAST            (self)
#             1536 LOAD_METHOD          (_update_jellyfish_effect)
#             1538 CALL_METHOD          (0 positional arguments)
#             1540 POP_TOP
#             1542 EXTENDED_ARG         (256)
#             1544 JUMP_FORWARD         (to 1990)
# 
# 2556:     >> 1546 LOAD_FAST            (effect)
#             1548 LOAD_CONST           ("type")
#             1550 BINARY_SUBSCR
#             1552 LOAD_CONST           ("koi_pond")
#             1554 COMPARE_OP           (==)
#             1556 EXTENDED_ARG         (1536)
#             1558 POP_JUMP_IF_FALSE    (to 1572)
# 
# 2557:        1560 LOAD_FAST            (self)
#             1562 LOAD_METHOD          (_update_koi_pond_effect)
#             1564 CALL_METHOD          (0 positional arguments)
#             1566 POP_TOP
#             1568 EXTENDED_ARG         (256)
#             1570 JUMP_FORWARD         (to 1990)
# 
# 2558:     >> 1572 LOAD_FAST            (effect)
#             1574 LOAD_CONST           ("type")
#             1576 BINARY_SUBSCR
#             1578 LOAD_CONST           ("lasers")
#             1580 COMPARE_OP           (==)
#             1582 EXTENDED_ARG         (1536)
#             1584 POP_JUMP_IF_FALSE    (to 1598)
# 
# 2559:        1586 LOAD_FAST            (self)
#             1588 LOAD_METHOD          (_update_lasers_effect)
#             1590 CALL_METHOD          (0 positional arguments)
#             1592 POP_TOP
#             1594 EXTENDED_ARG         (256)
#             1596 JUMP_FORWARD         (to 1990)
# 
# 2560:     >> 1598 LOAD_FAST            (effect)
#             1600 LOAD_CONST           ("type")
#             1602 BINARY_SUBSCR
#             1604 LOAD_CONST           ("lava_lamp")
#             1606 COMPARE_OP           (==)
#             1608 EXTENDED_ARG         (1536)
#             1610 POP_JUMP_IF_FALSE    (to 1624)
# 
# 2561:        1612 LOAD_FAST            (self)
#             1614 LOAD_METHOD          (_update_lava_lamp_effect)
#             1616 CALL_METHOD          (0 positional arguments)
#             1618 POP_TOP
#             1620 EXTENDED_ARG         (256)
#             1622 JUMP_FORWARD         (to 1990)
# 
# 2562:     >> 1624 LOAD_FAST            (effect)
#             1626 LOAD_CONST           ("type")
#             1628 BINARY_SUBSCR
#             1630 LOAD_CONST           ("mario")
#             1632 COMPARE_OP           (==)
#             1634 EXTENDED_ARG         (1536)
#             1636 POP_JUMP_IF_FALSE    (to 1650)
# 
# 2563:        1638 LOAD_FAST            (self)
#             1640 LOAD_METHOD          (_update_mario_effect)
#             1642 CALL_METHOD          (0 positional arguments)
#             1644 POP_TOP
#             1646 EXTENDED_ARG         (256)
#             1648 JUMP_FORWARD         (to 1990)
# 
# 2564:     >> 1650 LOAD_FAST            (effect)
#             1652 LOAD_CONST           ("type")
#             1654 BINARY_SUBSCR
#             1656 LOAD_CONST           ("multiverse_simple")
#             1658 COMPARE_OP           (==)
#             1660 EXTENDED_ARG         (1536)
#             1662 POP_JUMP_IF_FALSE    (to 1676)
# 
# 2565:        1664 LOAD_FAST            (self)
#             1666 LOAD_METHOD          (_update_multiverse_simple_effect)
#             1668 CALL_METHOD          (0 positional arguments)
#             1670 POP_TOP
#             1672 EXTENDED_ARG         (256)
#             1674 JUMP_FORWARD         (to 1990)
# 
# 2566:     >> 1676 LOAD_FAST            (effect)
#             1678 LOAD_CONST           ("type")
#             1680 BINARY_SUBSCR
#             1682 LOAD_CONST           ("cosmic_nebula")
#             1684 COMPARE_OP           (==)
#             1686 EXTENDED_ARG         (1536)
#             1688 POP_JUMP_IF_FALSE    (to 1702)
# 
# 2567:        1690 LOAD_FAST            (self)
#             1692 LOAD_METHOD          (render_cosmic_nebula)
#             1694 CALL_METHOD          (0 positional arguments)
#             1696 POP_TOP
#             1698 EXTENDED_ARG         (256)
#             1700 JUMP_FORWARD         (to 1990)
# 
# 2568:     >> 1702 LOAD_FAST            (effect)
#             1704 LOAD_CONST           ("type")
#             1706 BINARY_SUBSCR
#             1708 LOAD_CONST           ("neon_grid")
#             1710 COMPARE_OP           (==)
#             1712 EXTENDED_ARG         (1536)
#             1714 POP_JUMP_IF_FALSE    (to 1728)
# 
# 2569:        1716 LOAD_FAST            (self)
#             1718 LOAD_METHOD          (render_neon_grid)
#             1720 CALL_METHOD          (0 positional arguments)
#             1722 POP_TOP
#             1724 EXTENDED_ARG         (256)
#             1726 JUMP_FORWARD         (to 1990)
# 
# 2570:     >> 1728 LOAD_FAST            (effect)
#             1730 LOAD_CONST           ("type")
#             1732 BINARY_SUBSCR
#             1734 LOAD_CONST           ("neurolink")
#             1736 COMPARE_OP           (==)
#             1738 EXTENDED_ARG         (1536)
#             1740 POP_JUMP_IF_FALSE    (to 1752)
# 
# 2571:        1742 LOAD_FAST            (self)
#             1744 LOAD_METHOD          (render_neurolink)
#             1746 CALL_METHOD          (0 positional arguments)
#             1748 POP_TOP
#             1750 JUMP_FORWARD         (to 1990)
# 
# 2572:     >> 1752 LOAD_FAST            (effect)
#             1754 LOAD_CONST           ("type")
#             1756 BINARY_SUBSCR
#             1758 LOAD_CONST           ("new_years")
#             1760 COMPARE_OP           (==)
#             1762 EXTENDED_ARG         (1536)
#             1764 POP_JUMP_IF_FALSE    (to 1776)
# 
# 2573:        1766 LOAD_FAST            (self)
#             1768 LOAD_METHOD          (render_new_years)
#             1770 CALL_METHOD          (0 positional arguments)
#             1772 POP_TOP
#             1774 JUMP_FORWARD         (to 1990)
# 
# 2574:     >> 1776 LOAD_FAST            (effect)
#             1778 LOAD_CONST           ("type")
#             1780 BINARY_SUBSCR
#             1782 LOAD_CONST           ("paint_splatters")
#             1784 COMPARE_OP           (==)
#             1786 EXTENDED_ARG         (1792)
#             1788 POP_JUMP_IF_FALSE    (to 1800)
# 
# 2575:        1790 LOAD_FAST            (self)
#             1792 LOAD_METHOD          (render_paint_splatters)
#             1794 CALL_METHOD          (0 positional arguments)
#             1796 POP_TOP
#             1798 JUMP_FORWARD         (to 1990)
# 
# 2576:     >> 1800 LOAD_FAST            (effect)
#             1802 LOAD_CONST           ("type")
#             1804 BINARY_SUBSCR
#             1806 LOAD_CONST           ("party_parrot")
#             1808 COMPARE_OP           (==)
#             1810 EXTENDED_ARG         (1792)
#             1812 POP_JUMP_IF_FALSE    (to 1824)
# 
# 2577:        1814 LOAD_FAST            (self)
#             1816 LOAD_METHOD          (render_party_parrot)
#             1818 CALL_METHOD          (0 positional arguments)
#             1820 POP_TOP
#             1822 JUMP_FORWARD         (to 1990)
# 
# 2578:     >> 1824 LOAD_FAST            (effect)
#             1826 LOAD_CONST           ("type")
#             1828 BINARY_SUBSCR
#             1830 LOAD_CONST           ("pixel_wave")
#             1832 COMPARE_OP           (==)
#             1834 EXTENDED_ARG         (1792)
#             1836 POP_JUMP_IF_FALSE    (to 1848)
# 
# 2579:        1838 LOAD_FAST            (self)
#             1840 LOAD_METHOD          (render_pixel_wave)
#             1842 CALL_METHOD          (0 positional arguments)
#             1844 POP_TOP
#             1846 JUMP_FORWARD         (to 1990)
# 
# 2580:     >> 1848 LOAD_FAST            (effect)
#             1850 LOAD_CONST           ("type")
#             1852 BINARY_SUBSCR
#             1854 LOAD_CONST           ("planetary_orbit")
#             1856 COMPARE_OP           (==)
#             1858 EXTENDED_ARG         (1792)
#             1860 POP_JUMP_IF_FALSE    (to 1872)
# 
# 2581:        1862 LOAD_FAST            (self)
#             1864 LOAD_METHOD          (render_planetary_orbit)
#             1866 CALL_METHOD          (0 positional arguments)
#             1868 POP_TOP
#             1870 JUMP_FORWARD         (to 1990)
# 
# 2582:     >> 1872 LOAD_FAST            (effect)
#             1874 LOAD_CONST           ("type")
#             1876 BINARY_SUBSCR
#             1878 LOAD_CONST           ("plasma_flow")
#             1880 COMPARE_OP           (==)
#             1882 EXTENDED_ARG         (1792)
#             1884 POP_JUMP_IF_FALSE    (to 1896)
# 
# 2583:        1886 LOAD_FAST            (self)
#             1888 LOAD_METHOD          (render_plasma_flow)
#             1890 CALL_METHOD          (0 positional arguments)
#             1892 POP_TOP
#             1894 JUMP_FORWARD         (to 1990)
# 
# 2584:     >> 1896 LOAD_FAST            (effect)
#             1898 LOAD_CONST           ("type")
#             1900 BINARY_SUBSCR
#             1902 LOAD_CONST           ("poison_max")
#             1904 COMPARE_OP           (==)
#             1906 EXTENDED_ARG         (1792)
#             1908 POP_JUMP_IF_FALSE    (to 1920)
# 
# 2585:        1910 LOAD_FAST            (self)
#             1912 LOAD_METHOD          (render_poison)
#             1914 CALL_METHOD          (0 positional arguments)
#             1916 POP_TOP
#             1918 JUMP_FORWARD         (to 1990)
# 
# 2586:     >> 1920 LOAD_FAST            (effect)
#             1922 LOAD_CONST           ("type")
#             1924 BINARY_SUBSCR
#             1926 LOAD_CONST           ("rainbow_tap")
#             1928 COMPARE_OP           (==)
#             1930 EXTENDED_ARG         (1792)
#             1932 POP_JUMP_IF_FALSE    (to 1944)
# 
# 2587:        1934 LOAD_FAST            (self)
#             1936 LOAD_METHOD          (render_rainbow_tap)
#             1938 CALL_METHOD          (0 positional arguments)
#             1940 POP_TOP
#             1942 JUMP_FORWARD         (to 1990)
# 
# 2588:     >> 1944 LOAD_FAST            (effect)
#             1946 LOAD_CONST           ("type")
#             1948 BINARY_SUBSCR
#             1950 LOAD_CONST           ("resident_evil")
#             1952 COMPARE_OP           (==)
#             1954 EXTENDED_ARG         (1792)
#             1956 POP_JUMP_IF_FALSE    (to 1968)
# 
# 2589:        1958 LOAD_FAST            (self)
#             1960 LOAD_METHOD          (render_resident_evil)
#             1962 CALL_METHOD          (0 positional arguments)
#             1964 POP_TOP
#             1966 JUMP_FORWARD         (to 1990)
# 
# 2590:     >> 1968 LOAD_FAST            (effect)
#             1970 LOAD_CONST           ("type")
#             1972 BINARY_SUBSCR
#             1974 LOAD_CONST           ("rick_morty")
#             1976 COMPARE_OP           (==)
#             1978 EXTENDED_ARG         (1792)
#             1980 POP_JUMP_IF_FALSE    (to 1990)
# 
# 2591:        1982 LOAD_FAST            (self)
#             1984 LOAD_METHOD          (render_rick_morty)
#             1986 CALL_METHOD          (0 positional arguments)
#             1988 POP_TOP
#          >> 1990 LOAD_CONST           (None)
#             1992 RETURN_VALUE
# 
# 
# ==== .LightEffectManager._update_rainbow_effect (linha 2593)
effect = self.effects["rainbow"]
effect["position"] += effect["speed"]
if effect["position"] >= self.width:
    effect["position"] = 0
for x in range(self.width):
    pos = (x + effect["position"]) % self.width / self.width
    if pos < 0.166:
        r = 255
        g = int(255 * pos * 6)
        b = 0
    elif pos < 0.333:
        r = int(255 * (2 - pos * 6))
        g = 255
        b = 0
    elif pos < 0.5:
        r = 0
        g = 255
        b = int(255 * (pos * 6 - 2))
    elif pos < 0.666:
        r = 0
        g = int(255 * (4 - pos * 6))
        b = 255
    elif pos < 0.833:
        r = int(255 * (pos * 6 - 4))
        g = 0
        b = 255
    else:
        r = 255
        g = 0
        b = int(255 * (6 - pos * 6))
    self.color_buffer[:, x] = [r, g, b]

self.color_buffer_flag = 1
# ==== .LightEffectManager._update_wave_effect (linha 2630)
effect = self.effects["wave"]
effect["position"] += effect["speed"]
if effect["position"] >= self.width:
    effect["position"] = 0
self.color_buffer.fill(0)
wave_color = effect["color"]
wave_width = effect["width"]
start = effect["position"]
end = start + wave_width
if end <= self.width:
    self.color_buffer[:, start:end] = wave_color
else:
    part1_width = self.width - start
    self.color_buffer[:, start:] = wave_color
    self.color_buffer[:, :end - self.width] = wave_color
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_block_effect (linha 2650)
effect = self.effects["block"]
effect["position"] += effect["speed"]
self.color_buffer.fill(0)
block_height = effect["height"]
total_blocks = 600 / effect["height"]
for i, block in enumerate(effect["fixed_blocks"]):
    start = self.height - (i + 1) * block_height
    end = self.height - i * block_height
    self.color_buffer[int(start):int(end), :] = effect["color"]

stop_position = self.height - len(effect["fixed_blocks"]) * block_height
if effect["current_block"] < total_blocks:
    current_pos = min(effect["position"], stop_position)
    current_start = max(0, current_pos - block_height)
    current_end = current_pos
    if len(effect["fixed_blocks"]) > 0:
        last_fixed_top = effect["fixed_blocks"][-1]["start"]
        current_end = min(current_end, last_fixed_top)
    if current_start < current_end:
        self.color_buffer[int(current_start):int(current_end), :] = effect["color"]
    if effect["position"] >= stop_position:
        if effect["current_block"] < total_blocks:
            fixed_start = stop_position - block_height
            fixed_end = stop_position
            effect["fixed_blocks"].append({'start':int(fixed_start), 
             'end':int(fixed_end)})
            effect["position"] = 0
            effect["current_block"] += 1
    if effect["current_block"] >= total_blocks:
        if effect["position"] >= block_height:
            effect["fixed_blocks"] = []
            effect["current_block"] = 0
            effect["position"] = 0
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_four_directions_effect (linha 2705)
effect = self.effects["four_directions"]
self.color_buffer.fill(0)
for i in range(effect["direction"]):
    color = effect["colors"][i]
    if i == 0:
        self.color_buffer[:, :] = color
    if i == 1:
        self.color_buffer[:, :] = color
    if i == 2:
        self.color_buffer[:, :] = color
    if i == 3:
        self.color_buffer[:, :] = color

if not effect["completed"]:
    effect["progress"] += effect["speed"]
    color = effect["colors"][effect["current_color"]]
    if effect["direction"] == 0:
        progress = min(effect["progress"], self.height)
        self.color_buffer[:progress, :] = color
        if progress >= self.height:
            effect["completed"] = True
    elif effect["direction"] == 1:
        progress = min(effect["progress"], self.width)
        self.color_buffer[:, self.width - progress:] = color
        if progress >= self.width:
            effect["completed"] = True
    elif effect["direction"] == 2:
        progress = min(effect["progress"], self.height)
        self.color_buffer[self.height - progress:, :] = color
        if progress >= self.height:
            effect["completed"] = True
    elif effect["direction"] == 3:
        progress = min(effect["progress"], self.width)
        self.color_buffer[:, :progress] = color
        if progress >= self.width:
            effect["completed"] = True
if effect["completed"]:
    effect["progress"] = 0
    effect["direction"] = (effect["direction"] + 1) % 4
    effect["current_color"] = (effect["current_color"] + 1) % 4
    effect["completed"] = False
# ==== .LightEffectManager._update_corner_fill_effect (linha 2759)
effect = self.effects["corner_fill"]
fill_step = effect["speed"] * 2
max_progress = int(np.sqrt(self.width ** 2 + self.height ** 2)) + 350
effect["progress"] = min(effect["progress"] + fill_step, max_progress)
self.color_buffer[:, :] = effect["colors"][effect["current_color"]]
next_color = effect["colors"][(effect["current_color"] + 1) % 4]
y, x = np.indices((self.height, self.width))
if effect["direction"] == 0:
    dist = self.width - 1 - x + (self.height - 1 - y)
elif effect["direction"] == 1:
    dist = self.width - 1 - x + y
elif effect["direction"] == 2:
    dist = x + y
elif effect["direction"] == 3:
    dist = x + (self.height - 1 - y)
mask = dist < effect["progress"]
self.color_buffer[mask] = next_color
if effect["progress"] >= max_progress:
    effect["progress"] = 0
    effect["current_color"] = (effect["current_color"] + 1) % 4
    effect["direction"] = (effect["direction"] + 1) % 4
    if effect["direction"] == 0:
        if effect["current_color"] == 0:
            self.color_buffer[:, :] = effect["colors"][0]
# ==== .LightEffectManager._update_random_circle_effect (linha 2803)
effect = self.effects["random_circle"]
self.color_buffer.fill(0)
center_x = effect["center_x"]
center_y = effect["center_y"]
radius = effect["current_radius"]
color = effect["color"]
alpha = effect["current_alpha"]
if radius > 0:
    start_x = max(0, int(center_x - radius))
    end_x = min(self.width, int(center_x + radius + 1))
    start_y = max(0, int(center_y - radius))
    end_y = min(self.height, int(center_y + radius + 1))
    radius_sq = radius * radius
    y_indices, x_indices = np.mgrid[start_y:end_y, start_x:end_x]
    dx = x_indices - center_x
    dy = y_indices - center_y
    dist_sq = dx * dx + dy * dy
    mask = dist_sq <= radius_sq
    dist = np.zeros_like(dist_sq, dtype=(np.float32))
    np.sqrt(dist_sq, where=mask, out=dist)
    dist_factor = np.maximum(0, 1.0 - dist / radius)
    alpha_factor = alpha / 255.0
    r = (color[0] * dist_factor * alpha_factor).astype(np.uint8)
    g = (color[1] * dist_factor * alpha_factor).astype(np.uint8)
    b = (color[2] * dist_factor * alpha_factor).astype(np.uint8)
    self.color_buffer[start_y:end_y, start_x:end_x][mask] = np.dstack((r, g, b))[mask]
speed = effect["speed"]
if effect["state"] == 0:
    effect["current_radius"] += speed
    if effect["current_radius"] >= effect["max_radius"]:
        effect["state"] = 1
elif effect["state"] == 1:
    effect["current_alpha"] -= speed
    if effect["current_alpha"] <= 0:
        effect["current_radius"] = 0
        effect["current_alpha"] = 255
        effect["state"] = 0
        effect["circle_count"] += 1
        effect["center_x"] = np.random.randint(100, 500)
        effect["center_y"] = np.random.randint(100, 500)
        effect["color"] = self._get_rainbow_color(np.random.randint(0, 255))
# ==== .LightEffectManager._update_fourth_dimension_effect (linha 2882)
effect = self.effects["fourth_dimension"]
effect["time_counter"] += effect["speed"] / 125.0
grid_size = effect["grid_size"]
radius = grid_size // 2
self.color_buffer.fill(0)
gap_y = int(np.cos(np.radians(30)) * grid_size)
grid_width = int(self.width // grid_size) + 1
grid_height = int(self.height // gap_y) + 3
center_x = self.width / 2
center_y = self.height / 2
for gy in range(grid_height):
    for gx in range(grid_width):
        if gy % 2 == 0:
            cx = gx * grid_size
        else:
            cx = gx * grid_size + grid_size / 2
        cy = gy * gap_y
        dx = cx - center_x
        dy = cy - center_y
        dist = np.sqrt(dx ** 2 + dy ** 2)
        amp = np.sin(dist / 100 + effect["time_counter"]) * 50
        ny = cy + amp
        hue = 360 * (gy / grid_height + gx / grid_width + effect["time_counter"]) % 360
        color = self.hsl_to_rgb(hue, 100, 50)
        self.draw_hexagon(int(cx), int(ny), radius, color)

self.color_buffer_flag = 1
# ==== .LightEffectManager.draw_hexagon (linha 2934)
points = []
for i in range(6):
    angle_deg = 60 * i + 30
    angle_rad = np.radians(angle_deg)
    x = cx + radius * np.cos(angle_rad)
    y = cy + radius * np.sin(angle_rad)
    points.append((int(x), int(y)))

self.fill_polygon(points, color)
# ==== .LightEffectManager.fill_polygon (linha 2947)
min_y = min((y for x, y in vertices))
max_y = max((y for x, y in vertices))
min_y = max(0, min_y)
max_y = min(self.height - 1, max_y)
for y in range(int(min_y), int(max_y) + 1):
    intersections = []
    n = len(vertices)
    for i in range(n):
        x1, y1 = vertices[i]
        x2, y2 = vertices[(i + 1) % n]
        if not not y1 <= y < y2:
            if not y2 <= y < y1:
                if y2 != y1:
                    x = x1 + (x2 - x1) * (y - y1) / (y2 - y1)
                    intersections.append(x)
                intersections.sort()

    for i in range(0, len(intersections), 2):
        if i + 1 < len(intersections):
            start_x = int(max(0, min(intersections[i], intersections[i + 1])))
            end_x = int(min(self.width - 1, max(intersections[i], intersections[i + 1])))
            start_x = max(0, start_x)
            end_x = min(self.width - 1, end_x)
            if start_x <= end_x:
                self.color_buffer[y, start_x:end_x + 1] = color
# ==== .LightEffectManager.hsl_to_rgb (linha 2991)
h /= 360.0
s /= 100.0
l /= 100.0
if s == 0:
    r = g = b = l
else:

    def hue_to_rgb(p, q, t):
        if t < 0:
            t += 1
        if t > 1:
            t -= 1
        if t < 0.16666666666666666:
            return p + (q - p) * 6 * t
        if t < 0.5:
            return q
        if t < 0.6666666666666666:
            return p + (q - p) * (0.6666666666666666 - t) * 6
        return p


    q = l * (1 + s) if l < 0.5 else l + s - l * s
    p = 2 * l - q
    r = hue_to_rgb(p, q, h + 0.3333333333333333)
    g = hue_to_rgb(p, q, h)
    b = hue_to_rgb(p, q, h - 0.3333333333333333)
r = int(r * 255)
g = int(g * 255)
b = int(b * 255)
return (
 r, g, b)
# ==== .LightEffectManager.hsl_to_rgb.hue_to_rgb (linha 3000)
if t < 0:
    t += 1
if t > 1:
    t -= 1
if t < 0.16666666666666666:
    return p + (q - p) * 6 * t
if t < 0.5:
    return q
if t < 0.6666666666666666:
    return p + (q - p) * (0.6666666666666666 - t) * 6
return p
# ==== .LightEffectManager._update_bubble_effect (linha 3019)
effect = self.effects["bubble"]
speed = effect["speed"]
for bubble in effect["bubbles"]:
    if "state" not in bubble:
        bubble["state"] = 0
    if "alpha" not in bubble:
        bubble["alpha"] = 255
    if "radius" not in bubble:
        bubble["radius"] = effect["min_radius"]
    if "color" not in bubble:
        bubble["color"] = effect["colors"][0]

for bubble in effect["bubbles"]:
    bubble["y"] -= speed * 2
    if bubble["state"] == 0:
        bubble["radius"] += speed
        if not bubble["radius"] >= effect["max_radius"]:
            bubble["state"] = 1
        if bubble["state"] == 1:
            bubble["alpha"] -= speed
            if bubble["alpha"] <= 0:
                bubble["state"] = 2
        effect["bubbles"] = [b for b in effect["bubbles"] if b["state"] != 2]

if len(effect["bubbles"]) < effect["max_count"]:
    if np.random.random() < 0.05:
        colors_lists = [
         [
          0, 255, 0], [0, 0, 255]]
        new_bubble = {'x':(np.random.randint)(0, self.width), 
         'y':(self.height) + 20, 
         'radius':(np.random.randint)(effect["min_radius"], effect["max_radius"]), 
         'alpha':255, 
         'state':0, 
         'color':effect["colors"][np.random.randint(0, 2)]}
        effect["bubbles"].append(new_bubble)
self.color_buffer.fill(0)
for bubble in effect["bubbles"]:
    center_x = int(bubble["x"])
    center_y = int(bubble["y"])
    radius = bubble["radius"]
    color = bubble["color"]
    alpha = bubble["alpha"]
    start_x = max(0, center_x - radius)
    end_x = min(self.width, center_x + radius + 1)
    start_y = max(0, center_y - radius)
    end_y = min(self.height, center_y + radius + 1)
    if not start_x >= end_x:
        if start_y >= end_y:
            pass
        else:
            try:
                y_indices, x_indices = np.mgrid[start_y:end_y, start_x:end_x]
                dx = x_indices - center_x
                dy = y_indices - center_y
                dist = np.sqrt(dx ** 2 + dy ** 2)
                mask = dist <= radius
                alpha_factor = np.where(mask, alpha / 255.0, 0)
                for c in range(3):
                    self.color_buffer[start_y:end_y, start_x:end_x, c][mask] = (color[c] * alpha_factor[mask]).astype(np.uint8)

            except ValueError as e:
                try:
                    print(f"绘制气泡时出错: {e}, 区域: ({start_x}-{end_x}, {start_y}-{end_y})")
                finally:
                    e = None
                    del e

        self.color_buffer_flag = 1
# ==== .LightEffectManager._update_dna_helix_links_effect (linha 3108)
effect = self.effects["dna_helix"]
speed = effect["speed"]
helix_color = np.array((effect["color"]), dtype=(np.uint8))
effect["position"] = effect.get("position", 0) + speed * 0.01
if effect["position"] >= 2 * np.pi:
    effect["position"] -= 2 * np.pi
self.color_buffer.fill(0)
amplitude = self.width // 2 * 0.8
vertical_spread = self.height // 5
num_points = max(1, self.width // 60)
base_radius = max(1, min(self.width, self.height) // 30)
dynamic_radius_range = base_radius - 2
helix_points = []
for i in range(num_points):
    phase_offset = 2 * np.pi * i / num_points
    point_time = effect["position"] + phase_offset
    x_pos = i * (self.width / (num_points - 1)) if num_points > 1 else self.width // 2
    radius_factor1 = np.cos(point_time)
    radius_factor2 = np.cos(point_time + np.pi)
    y1 = self.height // 2 + np.sin(point_time) * vertical_spread
    radius1 = base_radius + int(dynamic_radius_range * radius_factor1)
    y2 = self.height // 2 + np.sin(point_time + np.pi) * vertical_spread
    radius2 = base_radius + int(dynamic_radius_range * radius_factor2)
    helix_points.append({'x':x_pos, 
     'y':y1, 
     'radius':max(1, radius1), 
     'is_first':True})
    helix_points.append({'x':x_pos, 
     'y':y2, 
     'radius':max(1, radius2), 
     'is_first':False})

first_helix_points = [p for p in helix_points if p["is_first"]]
for i in range(len(first_helix_points) - 1):
    p1 = first_helix_points[i]
    p2 = first_helix_points[i + 1]
    self._draw_thick_line((p1["x"]),
      (p1["y"]), (p2["x"]),
      (p2["y"]), helix_color,
      thickness=5)

second_helix_points = [p for p in helix_points if not p["is_first"]]
for i in range(len(second_helix_points) - 1):
    p1 = second_helix_points[i]
    p2 = second_helix_points[i + 1]
    self._draw_thick_line((p1["x"]),
      (p1["y"]), (p2["x"]),
      (p2["y"]), helix_color,
      thickness=5)

for i in range(0, len(helix_points), 2):
    if i + 1 < len(helix_points):
        p1 = helix_points[i]
        p2 = helix_points[i + 1]
        self._draw_thick_line((p1["x"]),
          (p1["y"]), (p2["x"]),
          (p2["y"]), helix_color,
          thickness=5)

for point in helix_points:
    center_x = int(point["x"])
    center_y = int(point["y"])
    radius = point["radius"]
    start_x = max(0, center_x - radius)
    end_x = min(self.width, center_x + radius + 1)
    start_y = max(0, center_y - radius)
    end_y = min(self.height, center_y + radius + 1)
    if not start_x >= end_x:
        if start_y >= end_y:
            pass
        else:
            try:
                y_indices, x_indices = np.mgrid[start_y:end_y, start_x:end_x]
                dx = x_indices - center_x
                dy = y_indices - center_y
                dist = np.sqrt(dx ** 2 + dy ** 2)
                mask = dist <= radius
                self.color_buffer[start_y:end_y, start_x:end_x][mask] = helix_color
            except Exception as e:
                try:
                    print(f"绘制DNA点时出错: {e}")
                finally:
                    e = None
                    del e

        self.color_buffer_flag = 1
# ==== .LightEffectManager._draw_thick_line (linha 3236)
dx = x1 - x0
dy = y1 - y0
length = max(1, np.sqrt(dx * dx + dy * dy))
if length > 0:
    nx = dx / length
    ny = dy / length
else:
    nx, ny = (0, 0)
px = -ny
py = nx
half_thickness = max(1, thickness // 2)
min_x = min(x0, x1) - half_thickness
max_x = max(x0, x1) + half_thickness
min_y = min(y0, y1) - half_thickness
max_y = max(y0, y1) + half_thickness
min_x = max(0, int(min_x))
max_x = min(self.width - 1, int(max_x))
min_y = max(0, int(min_y))
max_y = min(self.height - 1, int(max_y))
if min_x >= max_x or min_y >= max_y:
    return
try:
    y_indices, x_indices = np.mgrid[min_y:max_y + 1, min_x:max_x + 1]
except:
    return None
else:
    vx = x_indices - x0
    vy = y_indices - y0
    proj_length = vx * nx + vy * ny
    distance = np.abs(vx * px + vy * py)
    mask = (proj_length >= 0) & (proj_length <= length) & (distance <= half_thickness)
    self.color_buffer[min_y:max_y + 1, min_x:max_x + 1][mask] = color
# ==== .LightEffectManager._update_radar_effect (linha 3299)
effect = self.effects["radar"]
speed = max(1, min(10, effect.get("speed", 5)))
fg_color = np.array((effect["colors"][0]), dtype=(np.uint8))
blip_color = np.array((effect["colors"][1]), dtype=(np.uint8))
ripple_color = np.array((effect["colors"][2]), dtype=(np.uint8))
bg_color = np.array((effect["colors"][3]), dtype=(np.uint8))
effect["angle"] += speed * 0.005
if effect["angle"] >= 2 * np.pi:
    effect["angle"] -= 2 * np.pi
self.color_buffer[:] = bg_color
center_x = self.width // 2
center_y = self.height // 2
max_radius = 600
end_x = int(center_x + np.cos(effect["angle"]) * max_radius)
end_y = int(center_y + np.sin(effect["angle"]) * max_radius)
self._draw_thick_line(center_x,
  center_y, end_x,
  end_y, fg_color,
  thickness=30)
center_radius = 15
start_x = max(0, center_x - center_radius)
end_x = min(self.width, center_x + center_radius + 1)
start_y = max(0, center_y - center_radius)
end_y = min(self.height, center_y + center_radius + 1)
if start_x < end_x:
    if start_y < end_y:
        y_indices, x_indices = np.mgrid[start_y:end_y, start_x:end_x]
        dx = x_indices - center_x
        dy = y_indices - center_y
        dist = np.sqrt(dx ** 2 + dy ** 2)
        mask = dist <= center_radius
        self.color_buffer[start_y:end_y, start_x:end_x][mask] = fg_color
    if effect["enable_blips"]:
        if np.random.random() < 0.025:
            angle = np.random.random() * 2 * np.pi
            distance = np.random.uniform(0.2, 0.9) * max_radius
            lifetime = np.random.randint(20, 50)
            effect["blips"].append({'angle':angle, 
             'distance':distance, 
             'lifetime':lifetime, 
             'initial_lifetime':lifetime, 
             'move_dir':1 if (np.random.random() > 0.5) else (-1), 
             'alpha':255})
        for blip in effect["blips"]:
            blip["lifetime"] -= 1
            if effect["move_blips"]:
                move_step = max_radius * 0.005
                blip["distance"] += move_step * blip["move_dir"]
                if blip["distance"] < 0.1 * max_radius or blip["distance"] > max_radius:
                    blip["move_dir"] *= -1
                    blip["distance"] = max(0.1 * max_radius, min(blip["distance"], max_radius))
                x = int(center_x + np.cos(blip["angle"]) * blip["distance"])
                y = int(center_y + np.sin(blip["angle"]) * blip["distance"])
                size = max(1, int(effect["blip_size"] * (blip["lifetime"] / blip["initial_lifetime"])))
                if size > 0:
                    if not 0 <= x < self.width:
                        if not 0 <= y < self.height:
                            start_x = max(0, x - size)
                            end_x = min(self.width, x + size + 1)
                            start_y = max(0, y - size)
                            end_y = min(self.height, y + size + 1)
                            if start_x < end_x:
                                if start_y < end_y:
                                    try:
                                        y_indices, x_indices = np.mgrid[start_y:end_y, start_x:end_x]
                                        dx = x_indices - x
                                        dy = y_indices - y
                                        dist = np.sqrt(dx ** 2 + dy ** 2)
                                        mask = dist <= size
                                        self.color_buffer[start_y:end_y, start_x:end_x][mask] = blip_color
                                    except Exception as e:
                                        try:
                                            print(f"绘制雷达光点时出错: {e}")
                                        finally:
                                            e = None
                                            del e

                                effect["blips"] = [b for b in effect["blips"] if b["lifetime"] > 0]

        if effect["enable_ripples"]:
            ripple_speed_multiplier = 2.0
            effect["ripple_timing"] += speed * ripple_speed_multiplier
            max_ripple_size = max_radius * 1.5
            if effect["ripple_timing"] < max_ripple_size:
                ripple_radius = effect["ripple_timing"]
                ripple_thickness = 30
                start_x = max(0, int(center_x - ripple_radius - ripple_thickness))
                end_x = min(self.width, int(center_x + ripple_radius + ripple_thickness) + 1)
                start_y = max(0, int(center_y - ripple_radius - ripple_thickness))
                end_y = min(self.height, int(center_y + ripple_radius + ripple_thickness) + 1)
                if start_x < end_x and start_y < end_y:
                    try:
                        y_indices, x_indices = np.mgrid[start_y:end_y, start_x:end_x]
                        dx = x_indices - center_x
                        dy = y_indices - center_y
                        dist = np.sqrt(dx ** 2 + dy ** 2)
                        mask = (dist >= ripple_radius) & (dist <= ripple_radius + ripple_thickness)
                        self.color_buffer[start_y:end_y, start_x:end_x][mask] = ripple_color
                    except Exception as e:
                        try:
                            print(f"绘制圆形涟漪效果时出错: {e}")
                        finally:
                            e = None
                            del e

            else:
                effect["ripple_timing"] = 0
# ==== .LightEffectManager._update_impact_effect (linha 3452)
effect = self.effects["impact"]
speed_factor = effect["speed"] * 2
self.color_buffer.fill(0)
ball_radius = effect["ball_radius"]
ball_x, ball_y = effect["ball_position"]
dx, dy = effect["ball_direction"]
magnitude = np.sqrt(dx ** 2 + dy ** 2)
if magnitude > 0:
    dx = dx / magnitude
    dy = dy / magnitude
new_x = ball_x + dx * speed_factor
new_y = ball_y + dy * speed_factor
collision = False
collision_side = None
if new_x - ball_radius < 0:
    collision = True
    new_x = ball_radius
    dx = abs(dx)
    collision_side = "left"
elif new_x + ball_radius > self.width:
    collision = True
    new_x = self.width - ball_radius
    dx = -abs(dx)
    collision_side = "right"
if new_y - ball_radius < 0:
    collision = True
    new_y = ball_radius
    dy = abs(dy)
    collision_side = "top"
elif new_y + ball_radius > self.height:
    collision = True
    new_y = self.height - ball_radius
    dy = -abs(dy)
    collision_side = "bottom"
effect["ball_position"] = [
 new_x, new_y]
effect["ball_direction"] = [dx, dy]
if collision:
    if collision_side == "left":
        impact_x = 0
        impact_y = new_y
    elif collision_side == "right":
        impact_x = self.width - 1
        impact_y = new_y
    elif collision_side == "top":
        impact_x = new_x
        impact_y = 0
    elif collision_side == "bottom":
        impact_x = new_x
        impact_y = self.height - 1
    else:
        impact_x = new_x
        impact_y = new_y
    effect["impact_position"] = [impact_x, impact_y]
    effect["impact_color"] = effect["ball_color"].copy()
    effect["impact_radius"] = effect["impact_effect_radius"]
    effect["impact_alpha"] = 255
    effect["last_collision_side"] = collision_side
    colors_temp = self._get_rainbow_color(np.random.randint(0, 255))
    effect["ball_color"] = colors_temp
    angle_adjust = np.random.uniform(-0.3, 0.3)
    new_angle = np.arctan2(dy, dx) + angle_adjust
    effect["ball_direction"] = [
     np.cos(new_angle),
     np.sin(new_angle)]
if effect["impact_position"]:
    impact_x, impact_y = effect["impact_position"]
    if effect["impact_color"] is None:
        effect["impact_color"] = [
         255, 0, 0]
    impact_color = np.array((effect["impact_color"]), dtype=(np.uint8))
    impact_radius = effect["impact_radius"]
    start_x = max(0, int(impact_x - impact_radius))
    end_x = min(self.width, int(impact_x + impact_radius + 1))
    start_y = max(0, int(impact_y - impact_radius))
    end_y = min(self.height, int(impact_y + impact_radius + 1))
    if start_x < end_x and start_y < end_y:
        try:
            y_indices, x_indices = np.mgrid[start_y:end_y, start_x:end_x]
            if y_indices.ndim == 1:
                y_indices = y_indices[:, np.newaxis]
            if x_indices.ndim == 1:
                x_indices = x_indices[:, np.newaxis]
            dx = x_indices - impact_x
            dy = y_indices - impact_y
            dist = np.sqrt(dx ** 2 + dy ** 2)
            mask = dist <= impact_radius
            sub_array = self.color_buffer[start_y:end_y, start_x:end_x]
            alpha_factor = 0.2
            r_val = (impact_color[0] * alpha_factor).astype(np.uint8)
            g_val = (impact_color[1] * alpha_factor).astype(np.uint8)
            b_val = (impact_color[2] * alpha_factor).astype(np.uint8)
            sub_array[mask] = [
             r_val, g_val, b_val]
        except Exception as e:
            try:
                print(f"绘制撞击效果时出错: {e}")
            finally:
                e = None
                del e

        else:
            effect["impact_alpha"] -= effect["speed"]
            effect["impact_radius"] += 1
            if effect["impact_alpha"] <= 0:
                effect["impact_position"] = None
                effect["impact_radius"] = 0
            ball_x, ball_y = effect["ball_position"]
            ball_color = np.array((effect["ball_color"]), dtype=(np.uint8))
            start_x = max(0, int(ball_x - ball_radius))
            end_x = min(self.width, int(ball_x + ball_radius + 1))
            start_y = max(0, int(ball_y - ball_radius))
            end_y = min(self.height, int(ball_y + ball_radius + 1))
            if start_x < end_x and start_y < end_y:
                try:
                    y_indices, x_indices = np.mgrid[start_y:end_y, start_x:end_x]
                    dx = x_indices - ball_x
                    dy = y_indices - ball_y
                    dist = np.sqrt(dx ** 2 + dy ** 2)
                    mask = dist <= ball_radius
                    self.color_buffer[start_y:end_y, start_x:end_x][mask] = ball_color
                    edge_mask = (dist > ball_radius * 0.8) & (dist <= ball_radius)
                    edge_alpha = (dist[edge_mask] - ball_radius * 0.8) / (ball_radius * 0.2)
                    edge_alpha = np.clip(edge_alpha, 0, 1)
                    r = (ball_color[0] * (1 - edge_alpha)).astype(np.uint8)
                    g = (ball_color[1] * (1 - edge_alpha)).astype(np.uint8)
                    b = (ball_color[2] * (1 - edge_alpha)).astype(np.uint8)
                    self.color_buffer[start_y:end_y, start_x:end_x][edge_mask] = np.dstack((r, g, b))[edge_mask]
                except Exception as e:
                    try:
                        pass
                    finally:
                        e = None
                        del e
# ==== .LightEffectManager._get_rainbow_color (linha 3654)
pos = pos % 255 / 255.0
if pos < 0.166:
    r = 255
    g = int(255 * pos * 6)
    b = 0
elif pos < 0.333:
    r = int(255 * (2 - pos * 6))
    g = 255
    b = 0
elif pos < 0.5:
    r = 0
    g = 255
    b = int(255 * (pos * 6 - 2))
elif pos < 0.666:
    r = 0
    g = int(255 * (4 - pos * 6))
    b = 255
elif pos < 0.833:
    r = int(255 * (pos * 6 - 4))
    g = 0
    b = 255
else:
    r = 255
    g = 0
    b = int(255 * (6 - pos * 6))
return [
 r, g, b]
# ==== .LightEffectManager._update_gradient_push_effect (linha 3685)
effect = self.effects["gradient_push"]
self.color_buffer.fill(0)
effect["rainbow_position"] = (effect["rainbow_position"] + effect["speed"]) % 255
rainbow_width = max(0, min(effect["rainbow_width"], self.width))
for x in range(rainbow_width):
    pos = (x + effect["rainbow_position"]) % 255
    color = self._get_rainbow_color(pos)
    fade_factor = 1.0 - x / rainbow_width
    r = int(color[0] * fade_factor)
    g = int(color[1] * fade_factor)
    b = int(color[2] * fade_factor)
    self.color_buffer[:, x] = [
     r, g, b]

band_width = effect["band_width"]
if not effect["band_active"]:
    rand_x = np.random.randint(0, min(effect["rainbow_width"], self.width))
    pos = (rand_x + effect["rainbow_position"]) % 255
    effect["band_color"] = self._get_rainbow_color(pos)
    effect["band_position"] = -band_width
    effect["band_active"] = True
effect["band_position"] += effect["speed"]
if effect["band_active"]:
    start_x = max(0, int(effect["band_position"]))
    end_x = min(self.width, int(effect["band_position"] + band_width))
    if effect["band_color"] is not None and len(effect["band_color"]) == 3:
        band_color = np.array((effect["band_color"]), dtype=(np.uint8))
        if end_x > start_x:
            self.color_buffer[:, start_x:end_x] = band_color
        if effect["band_position"] >= self.width:
            effect["band_active"] = False
self.color_buffer_flag = 1
# ==== .LightEffectManager.log_symmetric_scale (linha 3747)
import math
if value <= min_val:
    return 1
if value >= max_val:
    return 100
mid_val = (min_val + max_val) / 2
if value < mid_val:
    normalized = (value - min_val) / (mid_val - min_val)
    log_value = math.log10(0.01 + 0.99 * normalized)
    mapped = 1 + 49 * (1 + log_value / 2)
else:
    normalized = (value - mid_val) / (max_val - mid_val)
    mapped = 50 + 50 * math.pow(normalized, 0.5)
return int(max(1, min(100, mapped)))
# ==== .LightEffectManager._update_music_visualizer_effect (linha 3777)
effect = self.effects["music_visualizer"]
speed = effect["speed"]
last_volume = effect["last_volume"]
effect["position"] += speed
if effect["position"] >= self.width:
    effect["position"] = 0
shm = shared_memory.SharedMemory("mouse_move")
shm_vaule = np.ndarray((4, ), dtype=(np.int32), buffer=(shm.buf))
volume = copy.deepcopy(shm_vaule[3])
shm.close()
if volume != 0:
    if volume not in self.sound_vaule_lists:
        self.sound_vaule_lists.append(volume)
    if self.sound_vaule_lists != []:
        self.sound_min = min(self.sound_vaule_lists)
        self.sound_max = max(self.sound_vaule_lists)
if self.sound_min != 0:
    if volume != 0:
        volume = self.log_symmetric_scale(volume, self.sound_min, self.sound_max)
    add_c = 5
    if last_volume < volume:
        last_volume = volume
        effect["last_volume"] = last_volume
    elif last_volume > add_c:
        last_volume -= add_c
        effect["last_volume"] = last_volume
    else:
        last_volume = 0
        effect["last_volume"] = last_volume
    display_width = int(last_volume * self.width / 100 * (effect["sensitivity"] / 50))
    display_width = max(0, min(display_width, self.width))
    self.color_buffer.fill(0)
    for x in range(display_width):
        pos = (x + effect["position"]) % self.width / self.width
        if pos < 0.166:
            r = 255
            g = int(255 * pos * 6)
            b = 0
        elif pos < 0.333:
            r = int(255 * (2 - pos * 6))
            g = 255
            b = 0
        elif pos < 0.5:
            r = 0
            g = 255
            b = int(255 * (pos * 6 - 2))
        elif pos < 0.666:
            r = 0
            g = int(255 * (4 - pos * 6))
            b = 255
        elif pos < 0.833:
            r = int(255 * (pos * 6 - 4))
            g = 0
            b = 255
        else:
            r = 255
            g = 0
            b = int(255 * (6 - pos * 6))
        self.color_buffer[self.height - x - 1, :] = [
         r, g, b]
# ==== .LightEffectManager._update_diagonal_visualizer_effect (linha 3907)
effect = self.effects["diagonal_visualizer"]
shm = shared_memory.SharedMemory("mouse_move")
shm_value = np.ndarray((4, ), dtype=(np.int32), buffer=(shm.buf))
volume = copy.deepcopy(shm_value[3])
shm.close()
if volume != 0:
    if volume not in self.sound_vaule_lists:
        self.sound_vaule_lists.append(volume)
    if self.sound_vaule_lists:
        self.sound_min = min(self.sound_vaule_lists)
        self.sound_max = max(self.sound_vaule_lists)
if self.sound_min != 0:
    if volume != 0:
        volume = self.log_symmetric_scale(volume, self.sound_min, self.sound_max)
    add_c = 5
    if effect["last_volume"] < volume:
        effect["last_volume"] = volume
    elif effect["last_volume"] > add_c:
        effect["last_volume"] -= add_c
    else:
        effect["last_volume"] = 0
    self.color_buffer.fill(0)
    if effect["last_volume"] == 0:
        return
diagonal = int(np.sqrt(self.width ** 2 + self.height ** 2)) + 350
display_length = int(effect["last_volume"] * diagonal / 100)
color = effect["color"]
breath_factor = (np.sin(time.time() * effect["speed"]) + 1) / 2
min_brightness = 0.2
breath_factor = min_brightness + (1 - min_brightness) * breath_factor
breath_color = [int(c * breath_factor) for c in color]
y, x = np.indices((self.height, self.width))
dist = self.width - 1 - x + (self.height - 1 - y)
mask = dist < display_length
self.color_buffer[mask] = breath_color
# ==== .LightEffectManager._update_music_center_side_effect (linha 3974)
effect = self.effects["music_center_side"]
shm = shared_memory.SharedMemory("mouse_move")
shm_value = np.ndarray((4, ), dtype=(np.int32), buffer=(shm.buf))
volume = copy.deepcopy(shm_value[3])
shm.close()
if volume != 0:
    if volume not in self.sound_vaule_lists:
        self.sound_vaule_lists.append(volume)
    if self.sound_vaule_lists:
        self.sound_min = min(self.sound_vaule_lists)
        self.sound_max = max(self.sound_vaule_lists)
if self.sound_min != 0 and volume != 0:
    volume = self.log_symmetric_scale(volume, self.sound_min, self.sound_max)
    if volume < 50:
        volume = volume / 2
    add_c = 1
    if effect["last_volume"] < volume:
        effect["last_volume"] = volume
    elif effect["last_volume"] > add_c:
        effect["last_volume"] -= add_c
    else:
        effect["last_volume"] = 0
    self.color_buffer.fill(0)
    if effect["last_volume"] == 0:
        return
    center_x = self.width // 2
    center_y = self.height // 2
    max_beam_width = min(self.width, self.height) // 2
    beam_width = int(max_beam_width * effect["last_volume"] / 100 * effect["intensity"])
    color = effect["color"]
    left_start = max(0, center_x - beam_width)
    self.color_buffer[:, left_start:center_x] = color
    right_end = min(self.width, center_x + beam_width)
    self.color_buffer[:, center_x:right_end] = color
# ==== .LightEffectManager._update_tongping_effect (linha 4039)
import cv2

def mosaic_effect(image, block_size=20):
    """
            使用OpenCV的高效马赛克实现方法
            
            参数:
            image: 输入图像 (numpy数组)
            block_size: 马赛克块大小
            
            返回:
            马赛克处理后的图像
            """
    h, w = image.shape[:2]
    small_w = max(1, w // block_size)
    small_h = max(1, h // block_size)
    small = cv2.resize(image, (small_w, small_h), interpolation=(cv2.INTER_LINEAR))
    mosaic = cv2.resize(small, (w, h), interpolation=(cv2.INTER_NEAREST))
    resized_img_np = np.array(mosaic)
    return resized_img_np


changer_vaule(0, 0, 1)
effect = self.effects["screen_effects"]
tongping_flag = effect["quse"]
temp = "screen_1"
h = 600
w = 600
existing_shm = shared_memory.SharedMemory(temp)
sct_img_np = np.ndarray((h, w, 3), dtype=(np.uint8), buffer=(existing_shm.buf))
sct_img_np = np.flip((sct_img_np[..., ::-1]), axis=0)
resized_img = mosaic_effect(sct_img_np)
self.color_buffer.fill(0)
if tongping_flag == 0:
    color = test.calculate_mean(resized_img)
    self.color_buffer[:, :] = color
    print(color)
else:
    self.color_buffer[:, :] = resized_img
existing_shm.close()
if port.fan_advanced_effect[11] == 1:
    port.renwulan_shanshuo()
# ==== .LightEffectManager._update_tongping_effect.mosaic_effect (linha 4041)
h, w = image.shape[:2]
small_w = max(1, w // block_size)
small_h = max(1, h // block_size)
small = cv2.resize(image, (small_w, small_h), interpolation=(cv2.INTER_LINEAR))
mosaic = cv2.resize(small, (w, h), interpolation=(cv2.INTER_NEAREST))
resized_img_np = np.array(mosaic)
return resized_img_np
# ==== .LightEffectManager._update_snake_effect (linha 4137)
effect = self.effects["snake"]
speed = effect["speed"] * 3
if not effect["snake_body"]:
    start_x = 150
    start_y = 300
    segment_count = effect["body_length"] // effect["segment_length"]
    for i in range(segment_count):
        effect["snake_body"].append([
         start_x - i * effect["segment_length"],
         start_y])

if effect["food_position"] is None:
    self._spawn_food(effect)
if not effect["path"]:
    self._find_path_to_food(effect)
if effect["path"]:
    next_pos = effect["path"][0]
    effect["path"].pop(0)
    head_x, head_y = effect["snake_body"][0]
    dx = next_pos[0] - head_x
    dy = next_pos[1] - head_y
    if abs(dx) > abs(dy):
        effect["snake_direction"] = [
         1 if dx > 0 else -1, 0]
    else:
        effect["snake_direction"] = [
         0, 1 if dy > 0 else -1]
head_x, head_y = effect["snake_body"][0]
dx, dy = effect["snake_direction"]
new_head = [
 head_x + dx * speed,
 head_y + dy * speed]
if new_head[0] < 0 or new_head[0] >= self.width or new_head[0] >= self.width or new_head[1] >= self.height:
    effect["snake_body"] = []
    effect["snake_direction"] = [
     1, 0]
    effect["food_position"] = None
    effect["path"] = []
    return
effect["snake_body"].insert(0, new_head)
if len(effect["snake_body"]) > effect["body_length"] // effect["segment_length"]:
    effect["snake_body"].pop()
food_x, food_y = effect["food_position"]
distance = np.sqrt((new_head[0] - food_x) ** 2 + (new_head[1] - food_y) ** 2)
if distance < effect["food_radius"] + effect["segment_length"]:
    effect["snake_color"] = effect["food_color"]
    self._spawn_food(effect)
    effect["path"] = []
self.color_buffer.fill(0)
snake_color = effect["snake_color"]
for segment in effect["snake_body"]:
    x, y = int(segment[0]), int(segment[1])
    start_x = max(0, x - effect["segment_length"] // 2)
    end_x = min(self.width, x + effect["segment_length"] // 2)
    start_y = max(0, y - effect["body_height"] // 2)
    end_y = min(self.height, y + effect["body_height"] // 2)
    if start_x < end_x:
        if start_y < end_y:
            self.color_buffer[start_y:end_y, start_x:end_x] = snake_color
        self._draw_food(effect)
        self.color_buffer_flag = 1
# ==== .LightEffectManager._find_path_to_food (linha 4240)
self._backup_path(effect)
# ==== .LightEffectManager._backup_path (linha 4359)
head_x, head_y = effect["snake_body"][0]
food_x, food_y = effect["food_position"]
segment_length = effect["segment_length"]
speed = effect["speed"]
path = []
if head_x != food_x:
    if head_x < food_x:
        for x in range(int(head_x), int(food_x), segment_length):
            path.append([x, head_y])

    else:
        for x in range(int(head_x), int(food_x), -segment_length):
            path.append([x, head_y])

    path.append([food_x, head_y])
if head_y != food_y:
    current_y = head_y
    if len(path) > 0:
        current_y = path[-1][1]
    if current_y < food_y:
        for y in range(int(current_y), int(food_y), segment_length):
            path.append([food_x, y])

    else:
        for y in range(int(current_y), int(food_y), -segment_length):
            path.append([food_x, y])

    path.append([food_x, food_y])
effect["path"] = path
# ==== .LightEffectManager._spawn_food (linha 4402)
self.rgb_coordinates = RGB_zuobiao
if self.rgb_coordinates:
    idx = np.random.randint(0, len(self.rgb_coordinates))
    effect["food_position"] = self.rgb_coordinates[idx]
    effect["food_color"] = [
     np.random.randint(0, 256),
     np.random.randint(0, 256),
     np.random.randint(0, 256)]
# ==== .LightEffectManager._draw_food (linha 4418)
if effect["food_position"] is None:
    return
food_color = effect["food_color"]
x, y = effect["food_position"]
radius = effect["food_radius"]
start_x = max(0, int(x - radius))
end_x = min(self.width, int(x + radius + 1))
start_y = max(0, int(y - radius))
end_y = min(self.height, int(y + radius + 1))
for dx in range(start_x, end_x):
    for dy in range(start_y, end_y):
        distance = np.sqrt((dx - x) ** 2 + (dy - y) ** 2)
        if distance <= radius:
            self.color_buffer[(dy, dx)] = food_color
# ==== .LightEffectManager._update_clock_effect (linha 4441)
effect = self.effects["clock"]
self.color_buffer.fill(0)
self.color_buffer[:, :] = effect["colors"][3]
current_time = time.time()
milliseconds = int((current_time - int(current_time)) * 1000)
time_struct = time.localtime(current_time)
hours = time_struct.tm_hour
minutes = time_struct.tm_min
seconds = time_struct.tm_sec
hour_angle = (hours % 12 + minutes / 60) * 360 / 12 - 90
minute_angle = (minutes + seconds / 60) * 360 / 60 - 90
second_angle = (seconds + milliseconds / 1000.0) * 360.0 / 60.0 - 90
center_x = self.width // 2
center_y = self.height // 2
hand_length = 600
self._draw_clock_hand(center_x, center_y, hour_angle, hand_length, effect["hour_hand_width"], effect["colors"][0])
self._draw_clock_hand(center_x, center_y, minute_angle, hand_length, effect["minute_hand_width"], effect["colors"][1])
self._draw_clock_hand(center_x, center_y, second_angle, hand_length, effect["second_hand_width"], effect["colors"][2])
self._draw_center_dot(center_x, center_y, 30)
self.color_buffer_flag = 1
# ==== .LightEffectManager._draw_clock_hand (linha 4497)
angle_rad = np.radians(angle)
end_x = center_x + length * np.cos(angle_rad)
end_y = center_y + length * np.sin(angle_rad)
perp_angle = angle_rad + np.pi / 2
offset_x = width / 2 * np.cos(perp_angle)
offset_y = width / 2 * np.sin(perp_angle)
points = [
 (
  center_x - offset_x, center_y - offset_y),
 (
  center_x + offset_x, center_y + offset_y),
 (
  end_x + offset_x, end_y + offset_y),
 (
  end_x - offset_x, end_y - offset_y)]
self.fill_polygon(points, color)
# ==== .LightEffectManager._draw_center_dot (linha 4523)
start_x = max(0, int(center_x - radius))
end_x = min(self.width, int(center_x + radius + 1))
start_y = max(0, int(center_y - radius))
end_y = min(self.height, int(center_y + radius + 1))
for y in range(start_y, end_y):
    for x in range(start_x, end_x):
        dx = x - center_x
        dy = y - center_y
        distance = np.sqrt(dx ** 2 + dy ** 2)
        if distance <= radius:
            self.color_buffer[(y, x)] = [
             255, 255, 255]
# ==== .LightEffectManager._update_fireworks_effect (linha 4589)
effect = self.effects["fireworks"]
speed = effect["speed"] * 3
self.color_buffer.fill(0)
if np.random.random() < 0.05:
    new_firework = {'type':"ball",  'x':(np.random.randint)(100, self.width - 100), 
     'y':self.height, 
     'target_y':(np.random.randint)(0, 200), 
     'speed':speed, 
     'size':(np.random.randint)(1, 20), 
     'color':effect["colors"][0], 
     'alpha':255}
    effect["fireworks"].append(new_firework)
to_remove = []
for i, firework in enumerate(effect["fireworks"]):
    if firework["type"] == "ball":
        firework["y"] -= speed
        self._draw_firework_ball(firework)
        if firework["y"] <= firework["target_y"]:
            self._create_explosion(effect, firework)
            to_remove.append(i)
    else:
        if firework["type"] == "particle":
            firework["x"] += firework["vx"]
            firework["y"] += firework["vy"]
            firework["alpha"] = max(0, firework["alpha"] - firework["fade_speed"])
            self._draw_firework_particle(firework)
            if firework["alpha"] <= 0:
                to_remove.append(i)

for i in sorted(to_remove, reverse=True):
    effect["fireworks"].pop(i)

self.color_buffer_flag = 1
# ==== .LightEffectManager._create_explosion (linha 4646)
for _ in range(effect["particle_count"]):
    angle = np.random.uniform(0, 2 * np.pi)
    speed = np.random.uniform(effect["explode_speed_min"], effect["explode_speed_max"])
    particle = {'type':"particle", 
     'x':firework["x"], 
     'y':firework["y"], 
     'vx':(np.cos(angle)) * speed, 
     'vy':(np.sin(angle)) * speed, 
     'size':effect["particle_size"], 
     'color':effect["colors"][1], 
     'alpha':255, 
     'fade_speed':(np.random.randint)(5, 15)}
    effect["fireworks"].append(particle)
# ==== .LightEffectManager._draw_firework_ball (linha 4667)
size = firework["size"]
x_min = int(max(0, firework["x"] - size))
x_max = int(min(self.width - 1, firework["x"] + size))
y_min = int(max(0, firework["y"] - size))
y_max = int(min(self.height - 1, firework["y"] + size))
color = firework["color"]
alpha = firework["alpha"] / 255.0
for y in range(y_min, y_max + 1):
    for x in range(x_min, x_max + 1):
        dx = x - firework["x"]
        dy = y - firework["y"]
        distance = np.sqrt(dx ** 2 + dy ** 2)
        if distance <= size:
            dist_factor = 1.0 - distance / size * 0.5
            r = int(color[0] * dist_factor * alpha)
            g = int(color[1] * dist_factor * alpha)
            b = int(color[2] * dist_factor * alpha)
            self.color_buffer[(y, x)] = [r, g, b]
# ==== .LightEffectManager._draw_firework_particle (linha 4698)
size = particle["size"]
x_min = int(max(0, particle["x"] - size))
x_max = int(min(self.width - 1, particle["x"] + size))
y_min = int(max(0, particle["y"] - size))
y_max = int(min(self.height - 1, particle["y"] + size))
color = particle["color"]
alpha = particle["alpha"] / 255.0
for y in range(y_min, y_max + 1):
    for x in range(x_min, x_max + 1):
        dx = x - particle["x"]
        dy = y - particle["y"]
        distance = np.sqrt(dx ** 2 + dy ** 2)
        if distance <= size:
            dist_factor = 1.0 - distance / size * 0.5
            r = int(color[0] * dist_factor * alpha)
            g = int(color[1] * dist_factor * alpha)
            b = int(color[2] * dist_factor * alpha)
            self.color_buffer[(y, x)] = [r, g, b]
# ==== .LightEffectManager._update_sound_bar_visualizer_effect (linha 4729)
effect = self.effects["sound_bar_visualizer"]
self.color_buffer.fill(0)
shm = shared_memory.SharedMemory("mouse_move")
shm_value = np.ndarray((4, ), dtype=(np.int32), buffer=(shm.buf))
volume = copy.deepcopy(shm_value[3])
effect["volume"] = int(volume)
shm.close()
effect["color_position"] += effect["speed"]
if effect["color_position"] >= self.width:
    effect["color_position"] = 0
active_bar = min(9, int(effect["volume"] / 10))
target_height = 60 + int(effect["volume"] / 100 * 540)
for i in range(10):
    if i == active_bar:
        if target_height == 60:
            effect["bars"][i] = 60
        else:
            temp_random_vaule = random.randint(target_height, 600)
            if not effect["bars"][i] < temp_random_vaule:
                effect["bars"][i] = temp_random_vaule
    else:
        effect["bars"][i] = max(60, effect["bars"][i] - effect["speed"])

for i in range(10):
    x_start = i * 60
    x_end = (i + 1) * 60
    color_pos = (effect["color_position"] + i * 60) % self.width / self.width
    color = self._get_rainbow_color(color_pos * 255)
    bar_height = int(effect["bars"][i])
    y_start = self.height - bar_height
    self.color_buffer[y_start:self.height, x_start:x_end] = color
# ==== .LightEffectManager._update_matrix_flow_effect (linha 4784)
effect = self.effects["matrix_flow"]
effect["position"] += effect["speed"] * 3
total_interval = effect["matrix_width"] + effect["spacing"]
if effect["position"] >= total_interval:
    effect["position"] -= total_interval
self.color_buffer[:, :] = effect["colors"][1]
angle_rad = np.radians(80)
cos45 = np.cos(angle_rad)
sin45 = np.sin(angle_rad)
y_indices, x_indices = np.mgrid[0:self.height, 0:self.width]
base_proj = x_indices * cos45 + y_indices * sin45
proj_distance = (base_proj - effect["position"]) % total_interval
mask = proj_distance < effect["matrix_width"]
self.color_buffer[mask] = effect["colors"][0]
# ==== .LightEffectManager._update_matrix_rain_effect (linha 4820)
effect = self.effects["matrix_rain"]
speed = effect["speed"]
self.color_buffer.fill(0)
current_time = time.time()
if effect["last_update"] == 0:
    effect["last_update"] = current_time
    return
time_delta = min(0.1, current_time - effect["last_update"])
effect["last_update"] = current_time
char_width = effect["font_size"]
char_height = effect["font_size"]
num_columns = self.width // char_width
if len(effect["drops"]) == 0:
    effect["drops"] = []
    for i in range(num_columns):
        effect["drops"].append({'x':i * char_width, 
         'y':(np.random.randint)(-100, 0), 
         'speed':(np.random.uniform)(1, speed), 
         'length':(np.random.randint)(300, 600), 
         'chars':[]})

    for drop in effect["drops"]:
        drop["y"] += np.random.uniform(speed, 10)
        if drop["y"] > self.height + drop["length"]:
            drop["y"] = np.random.randint(-100, 0)
            drop["speed"] = np.random.uniform(30, 100)
            drop["length"] = np.random.randint(300, 600)
            drop["chars"] = []
        start_y = max(0, int(drop["y"]))
        end_y = min(self.height, int(drop["y"] + drop["length"]))
        if start_y < end_y:
            rel_y_start = (start_y - drop["y"]) / drop["length"]
            rel_y_end = (end_y - drop["y"]) / drop["length"]
            num_chars = (end_y - start_y) // char_height
        if num_chars <= 0:
            pass
        else:
            if len(drop["chars"]) < num_chars:
                drop["chars"] = [np.random.choice(list(effect["chars"])) for _ in range(num_chars)]
            for i in range(num_chars):
                char_y = start_y + i * char_height
                rel_pos = rel_y_start + i * char_height / drop["length"]
                brightness = 1.0 - rel_pos
                brightness = max(0, min(1, brightness))
                char_color = [int(c * brightness) for c in effect["color"]]
                char_x = drop["x"]
                if char_x < self.width:
                    if char_y < self.height:
                        end_x = min(self.width, char_x + char_width)
                        end_y_char = min(self.height, char_y + char_height)
                        if char_x < end_x:
                            if char_y < end_y_char:
                                self.color_buffer[char_y:end_y_char, char_x:end_x] = char_color
# ==== .LightEffectManager._update_fluid_fill_effect (linha 4912)
effect = self.effects["fluid_fill"]
buf_w = 100
buf_h = 100
if not hasattr(self, "fluid_buf"):
    self.fluid_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.fluid_buf[:] = 0
max_distance = np.sqrt(buf_w ** 2 + buf_h ** 2) + 50
effect["progress"] += effect["speed"] / 2
if effect["progress"] > max_distance:
    effect["progress"] = 0
    effect["stage"] = (effect["stage"] + 1) % 2
if effect["stage"] == 0:
    self.fluid_buf.fill(0)
if effect["stage"] == 0:
    fill_color = np.array(effect["color"])
else:
    fill_color = np.array([0, 0, 0])
wave_amp = effect["wave_height"]
wave_freq = 2 * np.pi / 100
effect["wave_offset"] += effect["wave_speed"]
if effect["wave_offset"] > 100:
    effect["wave_offset"] = 0
y, x = np.indices((buf_h, buf_w))
dist = np.sqrt((x - buf_w) ** 2 + (y - buf_h) ** 2)
wave_value = (np.sin((x + effect["wave_offset"]) * wave_freq) + 1) * 0.5 * wave_amp
fill_mask = dist <= effect["progress"]
wave_mask = (dist > effect["progress"] - wave_amp) & fill_mask
gradient_mask = np.clip((effect["progress"] - dist) / wave_amp, 0, 1)
self.fluid_buf[fill_mask] = fill_color
if effect["stage"] == 0:
    wave_color = np.clip(fill_color * 1.5, 0, 255).astype(np.uint8)
    self.fluid_buf[wave_mask] = wave_color
for c in range(3):
    self.fluid_buf[:, :, c] = np.where(fill_mask, np.clip(self.fluid_buf[:, :, c] * gradient_mask, 0, 255), self.fluid_buf[:, :, c])

h, w = self.height, self.width
x_idx = np.linspace(0, buf_w - 1, w).astype(np.int32)
y_idx = np.linspace(0, buf_h - 1, h).astype(np.int32)
self.color_buffer[:] = self.fluid_buf[(y_idx[:, None], x_idx)]
# ==== .LightEffectManager._update_box_pushing_effect (linha 4997)
effect = self.effects["box_pushing"]
box_size = 100
grid_cols = self.width // box_size
grid_rows = self.height // box_size
if not hasattr(self, "box_tuixiangzi"):
    self.box_tuixiangzi = None
if self.box_tuixiangzi is None:
    effect["grid"] = [[None for _ in range(grid_cols)] for _ in range(grid_rows)]
    effect["active_box"] = self._generate_new_box(effect)
    self.box_tuixiangzi = effect["grid"]
self.color_buffer.fill(0)
if effect["state"] == 0:
    if effect["active_box"]:
        x, y, target_col, row = effect["active_box"]
        x += effect["speed"]
        target_x = target_col * box_size
        if x >= target_x:
            if effect["grid"][row][target_col] is None:
                effect["grid"][row][target_col] = effect["color"][:]
                effect["active_box"] = self._generate_new_box(effect)
            else:
                new_target_col = -1
                for col in range(target_col - 1, -1, -1):
                    if effect["grid"][row][col] is None:
                        new_target_col = col
                        break

                if new_target_col >= 0:
                    effect["active_box"] = (x, y, new_target_col, row)
                else:
                    current_col = min(int(x // box_size), grid_cols - 1)
                    if effect["grid"][row][current_col] is None:
                        effect["grid"][row][current_col] = effect["color"][:]
                        effect["active_box"] = self._generate_new_box(effect)
                    else:
                        found = False
                        for col in range(current_col - 1, -1, -1):
                            if effect["grid"][row][col] is None:
                                effect["grid"][row][col] = effect["color"][:]
                                effect["active_box"] = self._generate_new_box(effect)
                                found = True
                                break

                        if not found:
                            effect["active_box"] = self._generate_new_box(effect)
        else:
            effect["active_box"] = (
             x, y, target_col, row)
    if self._is_grid_full(effect["grid"]):
        effect["state"] = 1
        effect["fade_progress"] = 0
elif effect["state"] == 1:
    effect["fade_progress"] = min(effect["fade_progress"] + effect["speed"], 255)
    if effect["fade_progress"] >= 255:
        effect["grid"] = [[None for _ in range(grid_cols)] for _ in range(grid_rows)]
        effect["state"] = 0
        effect["active_box"] = self._generate_new_box(effect)
        effect["fade_progress"] = 0
for row in range(grid_rows):
    for col in range(grid_cols):
        if effect["grid"][row][col]:
            color = effect["grid"][row][col][:]
            if effect["state"] == 1:
                fade_amount = min(effect["fade_progress"], 255)
                for c in range(3):
                    color[c] = max(0, color[c] - fade_amount)

                x_start = col * box_size
                y_start = row * box_size
                x_end = (col + 1) * box_size
                y_end = (row + 1) * box_size
                self.color_buffer[int(y_start):int(y_end), int(x_start):int(x_end)] = color

if effect["state"] == 0:
    if effect["active_box"]:
        x, y, _, _ = effect["active_box"]
        x_end = x + box_size
        y_end = y + box_size
        x_end = min(x_end, self.width)
        y_end = min(y_end, self.height)
        self.color_buffer[int(y):int(y_end), int(x):int(x_end)] = effect["color"]
self.color_buffer_flag = 1
# ==== .LightEffectManager._generate_new_box (linha 5113)
box_size = effect["box_size"]
grid_cols = self.width // box_size
grid_rows = self.height // box_size
row = effect["current_row"]
effect["current_row"] = row + 1
if effect["current_row"] > 5:
    effect["current_row"] = 0
available_cols = [col for col in range(grid_cols) if effect["grid"][row][col] is None]
if available_cols:
    target_col = max(available_cols)
    x = 0
    y = row * box_size
    return (
     x, y, target_col, row)
for r in range(grid_rows):
    available_cols = [col for col in range(grid_cols) if effect["grid"][r][col] is None]
    if available_cols:
        target_col = max(available_cols)
        x = 0
        y = r * box_size
        return (
         x, y, target_col, r)
# ==== .LightEffectManager._is_grid_full (linha 5149)
for row in grid:
    for cell in row:
        if cell is None:
            return False

    return True
# ==== .LightEffectManager._update_black_hole_effect (linha 5157)
effect = self.effects["black_hole"]
speed = effect.get("speed", 1) * 0.02
colors = np.array((effect.get("colors", [
 [
  255, 0, 0], [0, 255, 255], [255, 255, 0], [255, 0, 255]])),
  dtype=(np.float32))
buf_w = 100
buf_h = 100
if not hasattr(self, "blackhole_buf"):
    self.blackhole_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.blackhole_buf[:] = 0
scale_x = buf_w / self.width
scale_y = buf_h / self.height
particle_count = 50
block_size = 40
hole_size = 70
tail_length = 15
max_dist = min(self.width, self.height) // 2 - 30
cx, cy = self.width // 2, self.height // 2
half_bs = block_size // 2
if not hasattr(self, "_bh_particles"):
    self._bh_particles = {'angle':(np.random.uniform)(0, 2 * np.pi, particle_count),  'dist':(np.random.uniform)(hole_size + 40, max_dist, particle_count), 
     'speed':(np.random.uniform)(0.9, 1.3, particle_count), 
     'color_idx':(np.random.randint)(0, 4, particle_count), 
     'positions':np.zeros((particle_count, tail_length, 2), dtype=(np.float32)), 
     'ptr':0}
part = self._bh_particles
color = colors[part["color_idx"]]
part["angle"] += speed * part["speed"]
part["dist"] = np.maximum(hole_size, part["dist"] - 0.15)
x = cx + part["dist"] * np.cos(part["angle"])
y = cy + part["dist"] * np.sin(part["angle"])
pos = np.stack([x, y], axis=1)
part["positions"][:, part["ptr"]] = pos
part["ptr"] = (part["ptr"] + 1) % tail_length
reset_mask = part["dist"] <= hole_size
part["dist"][reset_mask] = max_dist
part["angle"][reset_mask] = np.random.uniform(0, 2 * np.pi, np.sum(reset_mask))
self.blackhole_buf.fill(0)
positions = part["positions"].copy()
alphas = np.linspace(0.05, 1.0, tail_length, dtype=(np.float32))
for i in range(tail_length):
    a = alphas[i]
    xs = positions[:, i, 0]
    ys = positions[:, i, 1]
    xs_buf = xs * scale_x
    ys_buf = ys * scale_y
    half_bs_buf = int(half_bs * min(scale_x, scale_y))
    x1 = np.clip(xs_buf - half_bs_buf, 0, buf_w).astype(int)
    y1 = np.clip(ys_buf - half_bs_buf, 0, buf_h).astype(int)
    x2 = np.clip(xs_buf + half_bs_buf, 0, buf_w).astype(int)
    y2 = np.clip(ys_buf + half_bs_buf, 0, buf_h).astype(int)
    for j in range(particle_count):
        if not x1[j] >= x2[j]:
            if y1[j] >= y2[j]:
                pass
            else:
                c = color[j]
                region = self.blackhole_buf[y1[j]:y2[j], x1[j]:x2[j]]
                self.blackhole_buf[y1[j]:y2[j], x1[j]:x2[j]] = (region * (1 - a) + c * a).astype(np.uint8)
        cx_buf = int(cx * scale_x)
        cy_buf = int(cy * scale_y)
        hole_size_buf = int(hole_size * min(scale_x, scale_y))
        self._draw_circle_safe_buf(cx_buf, cy_buf, hole_size_buf, 0, 0, 0)
        h, w = self.height, self.width
        x_idx = np.linspace(0, buf_w - 1, w).astype(np.int32)
        y_idx = np.linspace(0, buf_h - 1, h).astype(np.int32)
        self.color_buffer[:] = self.blackhole_buf[(y_idx[:, None], x_idx)]
        self.color_buffer_flag = 1
# ==== .LightEffectManager._draw_circle_safe_buf (linha 5278)
y, x = np.ogrid[-radius:radius, -radius:radius]
mask = x * x + y * y <= radius * radius
try:
    self.blackhole_buf[cy - radius:cy + radius, cx - radius:cx + radius][mask] = [
     r, g, b]
except:
    pass
# ==== .LightEffectManager._update_custom_wave_effect (linha 5287)
effect = self.effects["custom_wave"]
speed = effect["speed"] / 2
vertical = effect.get("vertical", False)
colors = effect["colors"]
if not hasattr(self, "_wave_offset"):
    self._wave_offset = 0.0
self._wave_offset += speed
w = self.width
h = self.height
band = w // 4
cycle = 4 * band
self._wave_offset %= cycle
offset = int(self._wave_offset)
self.color_buffer[:] = colors[0]
for i in range(-4, 8):
    x = offset + i * band
    c = colors[i % 4]
    if not vertical:
        x1 = max(0, x)
        x2 = min(w, x + band)
        if x1 < x2:
            self.color_buffer[:, x1:x2] = c
    else:
        y1 = max(0, x)
        y2 = min(h, x + band)
        if y1 < y2:
            self.color_buffer[y1:y2, :] = c
        self.color_buffer_flag = 1
# ==== .LightEffectManager._update_fire_effect (linha 5330)
effect = self.effects["fire"]
speed = max(1, effect["speed"])
colors = np.array((effect["colors"]), dtype=(np.uint8))
w = self.width
h = self.height
if not hasattr(self, "_fire_buf"):
    self._fire_w = w // 4
    self._fire_h = h // 4
    self._fire_buf = np.zeros((self._fire_h, self._fire_w), dtype=(np.uint8))
    self._fire_buf[-1, :] = 255
buf = self._fire_buf
fw, fh = self._fire_w, self._fire_h
if not hasattr(self, "_fire_cnt"):
    self._fire_cnt = 0
self._fire_cnt += 1
if self._fire_cnt < 11 - speed:
    return
self._fire_cnt = 0
for y in range(fh - 2, -1, -1):
    rnd = np.random.randint(0, 4, size=fw)
    nx = np.clip(np.arange(fw) - (rnd & 1) + (rnd >> 1), 0, fw - 1)
    buf[(y, nx)] = np.maximum(0, buf[(y + 1, nx)] - np.random.randint(0, 4, size=fw))

buf[-1, :] = 255
mask1 = buf < 64
mask2 = (buf >= 64) & (buf < 128)
mask3 = (buf >= 128) & (buf < 192)
mask4 = buf >= 192
fire_render = np.zeros((fh, fw, 3), dtype=(np.uint8))
fire_render[mask1] = colors[0]
fire_render[mask2] = colors[1]
fire_render[mask3] = colors[2]
fire_render[mask4] = colors[3]
self.color_buffer[:] = np.kron(fire_render, np.ones((4, 4, 1), dtype=(np.uint8)))
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_aurora_effect (linha 5401)
effect = self.effects["aurora"]
speed = effect["speed"] / 10
buf_w = 160
buf_h = 100
if not hasattr(self, "aurora_buf"):
    self.aurora_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.aurora_time = 0.0
self.aurora_time += speed * 0.1
y_arr, x_arr = np.meshgrid((np.linspace(0, 1, buf_h)),
  (np.linspace(0, 1, buf_w)),
  indexing="ij")
n1 = np.sin(x_arr * 8.0 + self.aurora_time * 1.2 + y_arr * 2.0) * 0.5 + 0.5
n2 = np.sin(x_arr * 4.0 + self.aurora_time * 0.7 + y_arr * 1.5) * 0.5 + 0.5
n3 = np.sin(x_arr * 2.0 + self.aurora_time * 0.3) * 0.5 + 0.5
noise = n1 * 0.5 + n2 * 0.3 + n3 * 0.2
mask = 1.0 - y_arr ** 1.6
mask = np.clip(mask, 0, 1)
alpha = noise * mask
r = (alpha * (180 - 100 * x_arr)).astype(np.uint8)
g = (alpha * (80 + 100 * x_arr)).astype(np.uint8)
b = (alpha * 220).astype(np.uint8)
self.aurora_buf[(Ellipsis, 0)] = r
self.aurora_buf[(Ellipsis, 1)] = g
self.aurora_buf[(Ellipsis, 2)] = b
h, w = self.height, self.width
x_idx = np.linspace(0, buf_w - 1, w).astype(np.int32)
y_idx = np.linspace(0, buf_h - 1, h).astype(np.int32)
self.color_buffer[:] = self.aurora_buf[(y_idx[:, None], x_idx)]
# ==== .LightEffectManager._update_riptide_wave_effect (linha 5465)
effect = self.effects["riptide_wave"]
speed = effect["speed"] / 12.0
bg_r, bg_g, bg_b = effect["colors"][0]
base_r, base_g, base_b = effect["colors"][1]
bubble_r, bubble_g, bubble_b = effect["colors"][2]
buf_w = 160
buf_h = 100
if not hasattr(self, "riptide_buf"):
    self.riptide_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.riptide_time = 0
    self.riptide_phase = 0.0
buf = self.riptide_buf
t = self.riptide_time
phase = self.riptide_phase
buf[:] = [
 bg_r, bg_g, bg_b]
self.riptide_time += 1
self.riptide_phase += speed * 0.08
reverse = t > 1350
if reverse:
    phase *= -2.5
    if t > 1600:
        self.riptide_time = 0
    y_arr, x_arr = np.meshgrid((np.linspace(0, 1, buf_h)),
      (np.linspace(0, 1, buf_w)),
      indexing="ij")
    wave1 = np.sin(x_arr * 7.0 + phase * 1.3) * 0.45 + 0.45
    wave2 = np.sin(x_arr * 14.0 + phase * 2.0) * 0.2 + 0.2
    wave3 = np.sin(x_arr * 3.0 + phase * 0.7) * 0.15 + 0.15
    wave = (wave1 + wave2 + wave3) * 0.8
    v_center = 0.5
    v_range = 0.22 if (not reverse) else 0.4
    y_wave = v_center + (y_arr - 0.5) * 0.15 - wave * v_range
    alpha = np.clip(1.0 - np.abs(y_arr - y_wave) * 8.0, 0.0, 0.6)
    buf[(Ellipsis, 0)] = (buf[(Ellipsis, 0)] * (1 - alpha) + base_r * alpha).astype(np.uint8)
    buf[(Ellipsis, 1)] = (buf[(Ellipsis, 1)] * (1 - alpha) + base_g * alpha).astype(np.uint8)
    buf[(Ellipsis, 2)] = (buf[(Ellipsis, 2)] * (1 - alpha) + base_b * alpha).astype(np.uint8)
    bubble_radius = 2
    bubble_count = 80
    if not hasattr(self, "bubble_xy"):
        self.bubble_xy = np.random.rand(bubble_count, 2) * [buf_w, buf_h]
    self.bubble_xy[:, 1] -= speed * 1.0 if (not reverse) else 3.0
    self.bubble_xy[:, 0] += np.random.randn(bubble_count) * 0.3
    reset_mask = self.bubble_xy[:, 1] < -bubble_radius * 3
    self.bubble_xy[(reset_mask, 0)] = np.random.rand(np.sum(reset_mask)) * buf_w
    self.bubble_xy[(reset_mask, 1)] = buf_h + np.random.rand(np.sum(reset_mask)) * 20
    xb = np.clip(self.bubble_xy[:, 0], 0, buf_w - 1).astype(np.int32)
    yb = np.clip(self.bubble_xy[:, 1], 0, buf_h - 1).astype(np.int32)
    y_g, x_g = np.ogrid[:buf_h, :buf_w]
    bubble_mask = np.zeros((buf_h, buf_w), dtype=bool)
    for i in range(len(xb)):
        cx, cy = xb[i], yb[i]
        bubble_mask |= (x_g - cx) ** 2 + (y_g - cy) ** 2 <= bubble_radius ** 2

buf[bubble_mask] = [
 bubble_r, bubble_g, bubble_b]
h, w = self.height, self.width
x_idx = np.linspace(0, buf_w - 1, w).astype(np.int32)
y_idx = np.linspace(0, buf_h - 1, h).astype(np.int32)
self.color_buffer[:] = buf[(y_idx[:, None], x_idx)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_gradient_effect (linha 5571)
effect = self.effects["gradient"]
speed = effect["speed"] / 180.0
buf_w = 100
buf_h = 100
if not hasattr(self, "grad_buf"):
    self.grad_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.grad_time = 0.0
buf = self.grad_buf
self.grad_time += speed
num_colors = effect["numColors"]
colors = np.array((effect["colors"]), dtype=(np.float32))
positions = np.array((effect["positions"]), dtype=(np.float32)) / 320.0
y_arr, x_arr = np.meshgrid((np.linspace(1, 0, buf_h)),
  (np.linspace(0, 1, buf_w)),
  indexing="ij")
t = (x_arr + y_arr) / 2.0
t = t + self.grad_time
t = t - np.floor(t)
out = np.zeros((buf_h, buf_w, 3), dtype=(np.float32))
for i in range(num_colors - 1):
    t0 = positions[i]
    t1 = positions[i + 1]
    c0 = colors[i]
    c1 = colors[i + 1]
    mask = (t >= t0) & (t <= t1)
    factor = (t[mask] - t0) / (t1 - t0 + 1e-08)
    out[mask] = c0 * (1 - factor[(Ellipsis, None)]) + c1 * factor[(Ellipsis, None)]

buf[...] = np.clip(out, 0, 255).astype(np.uint8)
h, w = self.height, self.width
x_idx = np.linspace(0, buf_w - 1, w).astype(np.int32)
y_idx = np.linspace(0, buf_h - 1, h).astype(np.int32)
self.color_buffer[:] = buf[(y_idx[:, None], x_idx)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_interleave_effect (linha 5639)
effect = self.effects["interleave"]
speed = effect["speed"] / 50.0
particle_size = 6
ring_thick = 6
expand_speed = 20
max_particles = 6
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
base_colors = [colors[1], colors[2], colors[3]]
buf_w = 100
buf_h = 100
if not hasattr(self, "interleave_buf"):
    self.interleave_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.il_step = 0.0
    self.il_particles = []
buf = self.interleave_buf
buf[:] = bg_color
y_arr, x_arr = np.meshgrid((np.linspace(0, 1, buf_h)), (np.linspace(0, 1, buf_w)), indexing="ij")
x_pix = x_arr * buf_w
y_pix = y_arr * buf_h
alive = []
for p in self.il_particles:
    x, y, vx, vy, r, max_r, ci = p
    x += vx * speed
    y += vy * speed
    if r < max_r:
        r += 0.5 * expand_speed
    if not x < -10:
        if not x > buf_w + 10:
            if not y < -10:
                if y > buf_h + 10:
                    pass
                else:
                    dist2 = (x_pix - x) ** 2 + (y_pix - y) ** 2
                    mask = (dist2 >= (r - ring_thick) ** 2) & (dist2 <= r ** 2)
                    buf[mask] = base_colors[ci]
                    alive.append([x,y,vx,vy,r,max_r,ci])
                shortage = max_particles - len(alive)

for _ in range(shortage):
    x = np.random.rand() * buf_w
    y = np.random.rand() * buf_h
    vx = (np.random.rand() * 2 - 1) * 0.8
    vy = (np.random.rand() * 2 - 1) * 3.0
    r = particle_size
    max_r = particle_size + np.random.rand() * 30
    ci = np.random.randint(0, 3)
    alive.append([x,y,vx,vy,r,max_r,ci])

self.il_particles = alive
h, w = self.height, self.width
x_idx = np.linspace(0, buf_w - 1, w).astype(int)
y_idx = np.linspace(0, buf_h - 1, h).astype(int)
self.color_buffer[:] = buf[(y_idx[:, None], x_idx)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_spiral_rainbow_effect (linha 5736)
effect = self.effects["spiral_rainbow"]
speed = effect["speed"] / 3.0
x_pos = effect["xPos"]
y_pos = effect["yPos"]
reversed_spin = effect["reversed"]
buf_w = 100
buf_h = 100
if not hasattr(self, "spiral_buf"):
    self.spiral_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.spiral_phase = 0.0
buf = self.spiral_buf
buf[:] = 0
dir = -1 if reversed_spin else 1
self.spiral_phase += dir * speed
y_arr, x_arr = np.meshgrid((np.linspace(0, 1, buf_h)),
  (np.linspace(0, 1, buf_w)),
  indexing="ij")
cx = x_pos / 320.0
cy = y_pos / 200.0
dx = x_arr - cx
dy = y_arr - cy
angle = np.arctan2(dy, dx)
hue = (angle / (2 * np.pi) + 0.5 + self.spiral_phase / 360.0) % 1.0
hue_deg = hue * 360 % 360
h = hue_deg / 60.0
c = 1.0
x_color = 1.0 - np.abs(h % 2 - 1.0)
r = np.zeros_like(h)
g = np.zeros_like(h)
b = np.zeros_like(h)
r = np.where((h >= 0) & (h < 1), c, r)
r = np.where((h >= 1) & (h < 2), x_color, r)
r = np.where((h >= 5) & (h < 6), c, r)
r = np.where((h >= 4) & (h < 5), x_color, r)
g = np.where((h >= 0) & (h < 1), x_color, g)
g = np.where((h >= 1) & (h < 3), c, g)
g = np.where((h >= 3) & (h < 4), x_color, g)
b = np.where((h >= 2) & (h < 3), x_color, b)
b = np.where((h >= 3) & (h < 5), c, b)
b = np.where((h >= 5) & (h < 6), c, b)
fade_mask = (h >= 5.8) | (h < 0.2)
r = np.where(fade_mask, r, r)
g = np.where(fade_mask, g * 0.5, g)
b = np.where(fade_mask, b * 0.3, b)
buf[(Ellipsis, 0)] = (r * 255).astype(np.uint8)
buf[(Ellipsis, 1)] = (g * 255).astype(np.uint8)
buf[(Ellipsis, 2)] = (b * 255).astype(np.uint8)
h_scr, w_scr = self.height, self.width
x_idx = np.linspace(0, buf_w - 1, w_scr).astype(np.int32)
y_idx = np.linspace(0, buf_h - 1, h_scr).astype(np.int32)
self.color_buffer[:] = buf[(y_idx[:, None], x_idx)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_meteor_effect (linha 5824)
effect = self.effects["meteor"]
speed = effect["speed"]
size = effect["size"]
path_diag = effect["path"]
colors = np.array((effect["colors"]), dtype=(np.uint8))
sky1 = colors[0]
sky2 = colors[1]
star_color = colors[2]
buf_w = 100
buf_h = 100
if not hasattr(self, "meteor_buf"):
    self.meteor_buf = np.zeros((buf_h, buf_w, 3), np.uint8)
    self.meteors = []
buf = self.meteor_buf
y_arr = np.linspace(0, 1, buf_h)[:, None]
for i in range(3):
    buf[(..., i)] = (sky1[i] * (1 - y_arr) + sky2[i] * y_arr).astype(np.uint8)

buf = np.clip(buf * 0.85, 0, 255).astype(np.uint8)
max_count = max(1, int(60 / size))
if len(self.meteors) < max_count:
    if np.random.rand() < 0.25:
        x = np.random.rand() * buf_w
        self.meteors.append({'x':x, 
         'y':-5,  'vx':0.0})
alive = []
move_speed = size * (speed / 50.0)
tail_length = max(3, int(size * 0.7))
y_coords, x_coords = np.mgrid[0:buf_h, 0:buf_w]
for m in self.meteors:
    x = m["x"]
    y = m["y"]
    vx = m["vx"]
    if path_diag:
        if np.random.randint(40) == 0:
            vx = move_speed * (1 if np.random.rand() > 0.5 else -1)
    else:
        vx = 0
    x += vx
    y += move_speed
    m["x"] = x
    m["y"] = y
    m["vx"] = vx
    cx = int(round(x))
    cy = int(round(y))
    r = max(1, size // 2)
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            d = abs(dx) + abs(dy)
            if d <= r:
                if not abs(dx) == abs(dy):
                    if not dx == 0:
                        if dy == 0:
                            pass
                px = cx + dx
                py = cy + dy
                if not 0 <= px < buf_w:
                    if not 0 <= py < buf_h:
                        buf[(py, px)] = star_color

    for i in range(1, tail_length):
        ty = y - i * 2
        tx = x - vx * i * 0.4
        px = int(round(tx))
        py = int(round(ty))
        if not 0 <= px < buf_w:
            if not 0 <= py < buf_h:
                buf[(py, px)] = np.clip(star_color * 0.7, 0, 255).astype(np.uint8)
            if y < buf_h + 50:
                alive.append(m)
            self.meteors = alive
            h_scr, w_scr = self.height, self.width
            x_idx = np.linspace(0, buf_w - 1, w_scr).astype(np.int32)
            y_idx = np.linspace(0, buf_h - 1, h_scr).astype(np.int32)
            self.color_buffer[:] = buf[(y_idx[:, None], x_idx)]
            self.color_buffer_flag = 1
# ==== .LightEffectManager._update_sky_cloud_effect (linha 5932)
effect = self.effects["sky_cloud"]
time_of_day = effect["timeOfDay"]
direction = effect["direction"]
cloud_speed = effect["speed"]
day_speed = effect["daySpeed"]
cloud_size = effect["size"]
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_day = colors[0]
cloud_day = colors[1]
bg_night = np.array([0, 14, 106], dtype=(np.uint8))
cloud_night = np.array([142, 155, 237], dtype=(np.uint8))
buf_w = 100
buf_h = 100
if not hasattr(self, "sky_buf"):
    self.sky_buf = np.zeros((buf_h, buf_w, 3), np.uint8)
    self.clouds = []
    self.time = 300
    self.is_day = True
    self.brightness = 50
    self.brightness2 = 97
buf = self.sky_buf
if time_of_day == 2:
    if self.time > 500 or self.time < -300:
        self.is_day = not self.is_day
    if self.is_day:
        self.time += day_speed / 4
        if self.brightness < 50:
            self.brightness += 1
            self.brightness2 += 1
    else:
        self.time -= day_speed / 4
        if self.brightness > 10:
            self.brightness -= 1
            self.brightness2 -= 1
    factor = self.brightness / 100.0
    current_bg = (bg_day * factor).astype(np.uint8)
    current_cloud = (cloud_day * (self.brightness2 / 100.0)).astype(np.uint8)
elif time_of_day == 0:
    current_bg = bg_day
    current_cloud = cloud_day
else:
    current_bg = bg_night
    current_cloud = cloud_night
buf[:] = current_bg
speed1 = cloud_speed / 4
max_clouds = 5
cloud_width = 25 + cloud_size * 1
if len(self.clouds) < max_clouds:
    if np.random.randint(0, 20) == 0:
        max_y = buf_h * 1.5 if direction == 2 else buf_h
        if direction == 0:
            x = np.random.uniform(-buf_w - cloud_width, -cloud_width)
        elif direction == 1:
            x = np.random.uniform(buf_w + cloud_width, buf_w * 2 + cloud_width)
        else:
            x = np.random.uniform(-buf_w - cloud_width, -cloud_width)
        y = np.random.uniform(0, max_y)
        self.clouds.append([x, y])
alive = []
w_add = int(cloud_size * 1)
h_add = int(cloud_size * 1)
for cloud in self.clouds:
    x, y = cloud
    if direction == 1:
        x -= speed1
    elif direction == 0:
        x += speed1
    else:
        x += speed1
        if x > buf_w * 0.6:
            y -= speed1
        elif x < buf_w * 0.15:
            y -= speed1
        else:
            y += speed1
    out_left = direction == 0 and x > buf_w + cloud_width
    out_right = direction == 1 and x < -cloud_width
    out_free = (direction == 2) and ((x > buf_w + cloud_width) or (x < -cloud_width))
    if not out_left:
        if not out_right:
            if out_free:
                pass
            else:
                cx = int(x)
                cy = int(y)
                w1 = 15 + w_add
                h1 = 5 + h_add
                x1 = cx + 5
                y1 = cy - 4
                self._draw_rect(buf, x1, y1, w1, h1, current_cloud)
                self._draw_rect(buf, cx, cy, 25 + w_add, 6 + h_add, current_cloud)
                alive.append([x, y])
            self.clouds = alive
            h_scr, w_scr = self.height, self.width
            x_idx = np.linspace(0, buf_w - 1, w_scr).astype(np.int32)
            y_idx = np.linspace(0, buf_h - 1, h_scr).astype(np.int32)
            self.color_buffer[:] = buf[(y_idx[:, None], x_idx)]
            self.color_buffer_flag = 1
# ==== .LightEffectManager._draw_rect (linha 6064)
buf_h, buf_w = buf.shape[:2]
x0 = max(0, int(x))
y0 = max(0, int(y))
x1 = min(buf_w, int(x + w))
y1 = min(buf_h, int(y + h))
if x0 < x1:
    if y0 < y1:
        buf[y0:y1, x0:x1] = color
# ==== .LightEffectManager._update_camo_fade_effect (linha 6079)
effect = self.effects["camo_fade"]
colors = np.array((effect["colors"]), dtype=(np.float32))
speed = effect["speed"] / 350.0
buf_w = 100
buf_h = 100
if not hasattr(self, "camo_buf"):
    self.camo_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.gradient_offset = 0.0
buf = self.camo_buf
self.gradient_offset += speed
self.gradient_offset %= 1.0
x_pos = (np.linspace(0.0, 1.0, buf_w) + self.gradient_offset) % 1.0
c0, c1, c2, c3 = colors
r = np.zeros_like(x_pos)
g = np.zeros_like(x_pos)
b = np.zeros_like(x_pos)
mask1 = x_pos <= 0.1
r[mask1] = c0[0]
g[mask1] = c0[1]
b[mask1] = c0[2]
mask2 = (x_pos > 0.1) & (x_pos <= 0.33)
t2 = (x_pos[mask2] - 0.1) / 0.23
r[mask2] = c0[0] * (1 - t2) + c1[0] * t2
g[mask2] = c0[1] * (1 - t2) + c1[1] * t2
b[mask2] = c0[2] * (1 - t2) + c1[2] * t2
mask3 = (x_pos > 0.33) & (x_pos <= 0.66)
t3 = (x_pos[mask3] - 0.33) / 0.33
r[mask3] = c1[0] * (1 - t3) + c2[0] * t3
g[mask3] = c1[1] * (1 - t3) + c2[1] * t3
b[mask3] = c1[2] * (1 - t3) + c2[2] * t3
mask4 = x_pos > 0.66
t4 = (x_pos[mask4] - 0.66) / 0.34
r[mask4] = c2[0] * (1 - t4) + c3[0] * t4
g[mask4] = c2[1] * (1 - t4) + c3[1] * t4
b[mask4] = c2[2] * (1 - t4) + c3[2] * t4
buf[:, :, 0] = r.astype(np.uint8)
buf[:, :, 1] = g.astype(np.uint8)
buf[:, :, 2] = b.astype(np.uint8)
h_scr, w_scr = self.height, self.width
x_idx = np.linspace(0, buf_w - 1, w_scr).astype(np.int32)
y_idx = np.linspace(0, buf_h - 1, h_scr).astype(np.int32)
self.color_buffer[:] = buf[(y_idx[:, None], x_idx)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_lasers_lasers_effect (linha 6157)
effect = self.effects["lasers_lasers"]
speed = effect["speed"] / 155.0
colors = np.array((effect["colors"]), dtype=(np.float32))
color_left = colors[0]
color_right = colors[1]
buf_w = 100
buf_h = 100
if not hasattr(self, "laser_simple_buf"):
    self.laser_simple_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.angle = 0.0
buf = self.laser_simple_buf
buf[:] = 0
self.angle += speed
if self.angle > np.pi / 2:
    self.angle = 0.0
lw = 5
cx, cy = 0, buf_h
dx = np.cos(self.angle) * 120
dy = -np.sin(self.angle) * 120
self._draw_line(buf, cx, cy, cx + dx, cy + dy, lw, color_left)
cx, cy = buf_w - 1, buf_h
dx = -np.cos(self.angle) * 120
dy = -np.sin(self.angle) * 120
self._draw_line(buf, cx, cy, cx + dx, cy + dy, lw, color_right)
h_scr, w_scr = self.height, self.width
x_idx = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
y_idx = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
self.color_buffer[:] = buf[(y_idx[:, None], x_idx)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._draw_line (linha 6201)
buf_h, buf_w = buf.shape[:2]
n = int(np.hypot(x2 - x1, y2 - y1)) or 1
xs = np.linspace(x1, x2, n, dtype=(np.int32))
ys = np.linspace(y1, y2, n, dtype=(np.int32))
hw = w // 2
r, g, b = color.astype(np.uint8)
for x, y in zip(xs, ys):
    for dx in range(-hw, hw + 1):
        for dy in range(-hw, hw + 1):
            xx, yy = x + dx, y + dy
            if not 0 <= xx < buf_w:
                if not 0 <= yy < buf_h:
                    buf[(yy, xx)] = (
                     r, g, b)
# ==== .LightEffectManager._update_storm_effect (linha 6224)
effect = self.effects["storm"]
speed = effect["speed"] / 10.0
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
rain1 = colors[1]
rain2 = colors[2]
lightning = np.array([255, 255, 255], np.uint8)
rain_size = 2
lightning_width = 2
enable_thunder = True
enable_lightning = True
random_bolts = True
buf_w = 100
buf_h = 100
if not hasattr(self, "storm_buf"):
    self.storm_buf = np.zeros((buf_h, buf_w, 3), np.uint8)
    self.raindrops = []
    self.lightning_bolts = []
    self.flash = 0
    self.thunder_timer = 0
buf = self.storm_buf
buf[:] = bg_color
if len(self.raindrops) < 60:
    if np.random.randint(0, 4) == 0:
        x = np.random.randint(0, buf_w)
        self.raindrops.append({'x':x, 
         'y':-5,  'speed':0.5 + (np.random.random() * 1.5), 
         'color':rain1 if (np.random.random() > 0.5) else rain2})
    alive = []
    for d in self.raindrops:
        d["y"] += d["speed"] * speed
        x = int(d["x"])
        y = int(d["y"])
        if not 0 <= x< buf_w:
            if 0 <= y < buf_h:
                buf[y:y + rain_size, x:x + rain_size] = d["color"]
            if d["y"] < buf_h + 10:
                alive.append(d)
            self.raindrops = alive
            self.thunder_timer += 1

    if enable_thunder:
        if self.thunder_timer > 240:
            self.thunder_timer = 0
            self.flash = 20
    if self.flash > 0:
        buf[:] = lightning
        self.flash -= 1
if enable_lightning and len(self.lightning_bolts) < 1:
    if np.random.randint(0, 80) == 0:
        x = np.random.randint(20, buf_w - 20)
        self.lightning_bolts.append({'x':x,  'life':12})
    bolt_alive = []
for b in self.lightning_bolts:
    x = b["x"]
    if random_bolts:
        points = [(x, 0), (x + np.random.randint(-4, 4), 25),
         (
          x + np.random.randint(-4, 4), 50), (x + np.random.randint(-4, 4), 75),
         (
          x, buf_h)]
        self._draw_bolt(buf, points, lightning_width, lightning)
    else:
        buf[:, x:x + lightning_width] = lightning
    b["life"] -= 1
    if b["life"] > 0:
        bolt_alive.append(b)
    self.lightning_bolts = bolt_alive
    h_scr, w_scr = self.height, self.width
    x_idx = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
    y_idx = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
    self.color_buffer[:] = buf[(y_idx[:, None], x_idx)]
    self.color_buffer_flag = 1
# ==== .LightEffectManager._draw_bolt (linha 6322)
buf_h, buf_w = buf.shape[:2]
for i in range(len(points) - 1):
    x1, y1 = points[i]
    x2, y2 = points[i + 1]
    n = max(5, int(np.hypot(x2 - x1, y2 - y1)))
    xs = np.linspace(x1, x2, n, dtype=(np.int32))
    ys = np.linspace(y1, y2, n, dtype=(np.int32))
    hw = width // 2
    for x, y in zip(xs, ys):
        for dx in range(-hw, hw + 1):
            xx = x + dx
            if not 0 <= xx < buf_w:
                if not 0 <= y < buf_h:
                    buf[(y, xx)] = color
# ==== .LightEffectManager._update_explode_effect (linha 6344)
effect = self.effects["explode"]
speed = effect["speed"]
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
main_color = colors[1]
max_bursts = 6
particle_cnt = 20
spread = 3
particle_size = 4
fade_start = 40
fade_step = 10
lifetime_min = 60
lifetime_max = 120
particle_move_speed = speed * 0.8
burst_expand_speed = speed * 0.08
fade_multiplier = speed * 1.2
lifetime_divisor = 1 + speed * 0.15
final_fade_start = fade_start / (1 + speed * 0.1)
final_fade_step = fade_step * fade_multiplier
final_lifetime_min = lifetime_min / lifetime_divisor
final_lifetime_max = lifetime_max / lifetime_divisor
move_divisor = (11.0 - spread) / particle_move_speed
buf_w = 100
buf_h = 100
if not hasattr(self, "explode_buf"):
    self.explode_buf = np.zeros((buf_h, buf_w, 3), np.uint8)
    self.bursts = []
    for _ in range(max_bursts):
        particles = []
        for _ in range(particle_cnt):
            vx = np.ceil(np.random.random() * 5) * (1 if np.random.random() > 0.5 else -1)
            vy = np.ceil(np.random.random() * 5) * (1 if np.random.random() > 0.5 else -1)
            particles.append({ 'x': 0, 'y': 0, 'vx': vx, 'vy': vy})

        self.bursts.append({'x':(np.random.randint)(5, buf_w - 5), 
         'y':(np.random.randint)(5, buf_h - 5), 
         'size':1.0, 
         'alpha':255, 
         'lifetime':(np.random.uniform)(final_lifetime_min, final_lifetime_max), 
         'particles':particles})

buf = self.explode_buf
buf[:] = bg_color
for burst in self.bursts:
    if burst["size"] > final_fade_start:
        burst["alpha"] = max(0, burst["alpha"] - final_fade_step)
    burst["size"] += burst_expand_speed
    r, g, b = main_color
    a = burst["alpha"] / 255.0
    draw_col = (int(r * a), int(g * a), int(b * a))
    for p in burst["particles"]:
        p["x"] += p["vx"] / move_divisor
        p["y"] += p["vy"] / move_divisor
        cx = int(burst["x"] + p["x"])
        cy = int(burst["y"] + p["y"])
        rad = particle_size
        for dy in range(-rad, rad + 1):
            for dx in range(-rad, rad + 1):
                if dx * dx + dy * dy <= rad * rad:
                    x = cx + dx
                    y = cy + dy
                    if not 0 <= x < buf_w:
                        if not 0 <= y < buf_h:
                            buf[(y, x)] = draw_col

            has_color = False

for y in range(buf_h):
    for x in range(buf_w):
        if not np.array_equal(buf[(y, x)], bg_color):
            has_color = True
            break

    if has_color:
        break

if not has_color:
    for burst in self.bursts:
        burst["size"] = 1.0
        burst["alpha"] = 255
        burst["x"] = np.random.randint(5, buf_w - 5)
        burst["y"] = np.random.randint(5, buf_h - 5)
        burst["lifetime"] = np.random.uniform(final_lifetime_min, final_lifetime_max)
        for p in burst["particles"]:
            p["x"] = 0.0
            p["y"] = 0.0

h_scr, w_scr = self.height, self.width
x_idx = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
y_idx = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
self.color_buffer[:] = buf[(y_idx[:, None], x_idx)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_rainbow_tunnel_effect (linha 6468)
effect = self.effects["rainbow_tunnel"]
speed = effect["speed"] * 0.2
ring_count = 32
dist = 26
line_width = 2
base_size = 5
size_step = 1
sat = 90
lite = 60
buf_w = 100
buf_h = 100
cx = buf_w // 2
cy = buf_h // 2
if not hasattr(self, "tunnel_buf"):
    self.tunnel_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.angle = 0.0
buf = self.tunnel_buf
buf[:] = 0
self.angle += speed
yidx, xidx = np.indices((buf_h, buf_w))
hw = line_width // 2
for i in range(ring_count):
    rad = np.radians(i * 5.0 + self.angle)
    x_pos = int(np.cos(rad) * np.sin(rad / 5) * dist + cx)
    y_pos = int(np.sin(rad) * np.cos(rad / 3) * dist + cy)
    radius = base_size + i * size_step
    hue = i * 10 % 360
    h = hue / 360.0
    s = sat / 100.0
    l = lite / 100.0

    def hue2rgb(p, q, t):
        if t < 0:
            t += 1
        if t > 1:
            t -= 1
        if t < 0.16666666666666666:
            return p + (q - p) * 6 * t
        if t < 0.5:
            return q
        if t < 0.6666666666666666:
            return p + (q - p) * (0.6666666666666666 - t) * 6
        return p


    if s == 0:
        r = g = b = int(l * 255)
    else:
        q = l * (1 + s) if l < 0.5 else l + s - l * s
        p = 2 * l - q
        r = int(hue2rgb(p, q, h + 0.3333333333333333) * 255)
        g = int(hue2rgb(p, q, h) * 255)
        b = int(hue2rgb(p, q, h - 0.3333333333333333) * 255)
    dx = xidx - x_pos
    dy = yidx - y_pos
    dist_sq = dx ** 2 + dy ** 2
    inner = (radius - hw) ** 2
    outer = (radius + hw) ** 2
    mask = (dist_sq >= inner) & (dist_sq <= outer)
    buf[mask] = (r, g, b)

h_scr, w_scr = self.height, self.width
x_map = np.linspace(0, buf_w - 1, w_scr).astype(np.int32)
y_map = np.linspace(0, buf_h - 1, h_scr).astype(np.int32)
self.color_buffer[:] = buf[(y_map[:, None], x_map)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_rainbow_tunnel_effect.hue2rgb (linha 6507)
if t < 0:
    t += 1
if t > 1:
    t -= 1
if t < 0.16666666666666666:
    return p + (q - p) * 6 * t
if t < 0.5:
    return q
if t < 0.6666666666666666:
    return p + (q - p) * (0.6666666666666666 - t) * 6
return p
# ==== .LightEffectManager._update_saiyan_effect (linha 6543)
effect = self.effects["saiyan"]
speed = effect["speed"] * 0.5
colors = np.array((effect["colors"]), dtype=(np.uint8))
bgColor = colors[0]
color1 = colors[1]
color2 = colors[2]
max_waves = 6
max_stars = 50
star_size = 3
wave_width = 6
wave_spacing = 6
buf_w = 100
buf_h = 100
half_w = buf_w // 2
half_h = buf_h // 2
if not hasattr(self, "saiyan_buf"):
    self.saiyan_buf = np.zeros((buf_h, buf_w, 3), np.uint8)
    self.waves_chaoji = []
    self.stars = []
buf = self.saiyan_buf
buf[:] = bgColor
while len(self.waves_chaoji) < max_waves:
    typ = np.random.choice(["left","right","left2","right2","up","down"])
    self.waves_chaoji.append({'type':typ, 
     'x':half_w if (typ in ('left', 'right', 'left2', 'right2')) else 0, 
     'y':buf_h if (typ == "up") else (0 if typ == "down" else 0), 
     'lifetime':150 if (typ in ('up', 'down')) else 50, 
     'rnd2':np.random.randint(100, 250) if ("2" in typ) else (np.random.randint(100, 150))})

alive_waves = []
for w in self.waves_chaoji:
    w["lifetime"] -= 1
    if w["lifetime"] <= 0:
        pass
    else:
        if w["type"] == "left" or w["type"] == "left2":
            w["x"] -= speed * 3
        else:
            if w["type"] == "right" or w["type"] == "right2":
                w["x"] += speed * 3
            if w["type"] == "up":
                if w["lifetime"] < 100:
                    w["y"] -= speed * 3
        if w["type"] == "down":
            if w["lifetime"] < 100:
                w["y"] += speed * 3
            x1, y1 = int(w["x"]), int(w["y"])
            r2 = w["rnd2"]
            if w["type"] in ('up', 'down'):
                x2, y2 = x1 + buf_w, y1
            else:
                x2 = x1 + r2
                y2 = y1 + buf_h
            hh, ww = buf.shape[:2]
            dx = abs(x2 - x1)
            dy = abs(y2 - y1)
            sx = 1 if x1 < x2 else -1
            sy = 1 if y1 < y2 else -1
            err = dx - dy
            r, g, b = color1
            hw = wave_width // 2
            tx, ty = x1, y1
            while True:
                for dyw in range(-hw, hw + 1):
                    for dxw in range(-hw, hw + 1):
                        cx = tx + dxw
                        cy = ty + dyw
                        if not 0 <= cx < ww:
                            if not 0 <= cy < hh:
                                buf[(cy, cx)] = (
                                 r, g, b)

                if tx == x2:
                    if ty == y2:
                        break
                    e2 = err * 2
                    if e2 > -dy:
                        err -= dy
                        tx += sx
                    if e2 < dx:
                        err += dx
                        ty += sy

            if w["type"] in ('up', 'down'):
                ny1 = y1 + wave_spacing
                ny2 = y2 + wave_spacing
                nx1, nx2 = x1, x2
            else:
                nx1 = x1 + wave_spacing
                nx2 = x2 + wave_spacing
                ny1, ny2 = y1, y2
            r2, g2, b2 = color2
            dx = abs(nx2 - nx1)
            dy = abs(ny2 - ny1)
            sx = 1 if nx1 < nx2 else -1
            sy = 1 if ny1 < ny2 else -1
            err = dx - dy
            tx, ty = nx1, ny1
            while True:
                for dyw in range(-hw, hw + 1):
                    for dxw in range(-hw, hw + 1):
                        cx = tx + dxw
                        cy = ty + dyw
                        if not 0 <= cx < ww:
                            if not 0 <= cy < hh:
                                buf[(cy, cx)] = (
                                 r2, g2, b2)

                if tx == nx2:
                    if ty == ny2:
                        break
                    e2 = err * 2
                    if e2 > -dy:
                        err -= dy
                        tx += sx
                    if e2 < dx:
                        err += dx
                        ty += sy

            if not -60 < w["x"] < buf_w + 60:
                alive_waves.append(w)
            self.waves_chaoji = alive_waves

while len(self.stars) < max_stars:
    self.stars.append({'x':(np.random.randint)(0, buf_w), 
     'y':(np.random.randint)(0, buf_h), 
     'lifetime':50, 
     'color':color1 if (np.random.random() > 0.5) else color2})

alive_stars = []
for s in self.stars:
    s["lifetime"] -= 1
    s["y"] += speed * 3
    x = int(s["x"])
    y = int(s["y"])
    if not 0 <= x< buf_w:
        if 0 <= y < buf_h:
            buf[y:y + star_size, x:x + star_size] = s["color"]
        if s["lifetime"] > 0:
            if s["y"] < buf_h + 50:
                alive_stars.append(s)
            self.stars = alive_stars
            h_scr, w_scr = self.height, self.width
            x_map = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
            y_map = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
            self.color_buffer[:] = buf[(y_map[:, None], x_map)]
            self.color_buffer_flag = 1
# ==== .LightEffectManager._update_spiral_effect (linha 6715)
# descompilação falhou (ParserError: Parse error at or near `LOAD_FAST' instruction at offset 0
); desmontagem:
# # pydisasm version 6.1.7
# # Python bytecode 3.8.0 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       _update_spiral_effect
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  34
# # Stack size:        6
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        6715
# # Constants:
# #    0: None
# #    1: 'spiral'
# #    2: 'speed'
# #    3: 0.9
# #    4: 'colors'
# #    5: ('dtype',)
# #    6: 0
# #    7: 1
# #    8: 2
# #    9: 3
# #   10: 100
# #   11: 50
# #   12: False
# #   13: 'spiral_buf_luoxuan'
# #   14: 0.0
# #   15: 360.0
# #   16: 360
# #   17: 0.5
# #   18: 1.0
# #   19: 45
# #   20: 180
# #   21: 225
# #   22: 90
# #   23: 270
# #   24: 135
# #   25: 315
# # Names:
# #    0: effects
# #    1: np
# #    2: array
# #    3: uint8
# #    4: hasattr
# #    5: zeros
# #    6: spiral_buf_luoxuan
# #    7: phase_spiral_effect
# #    8: pi
# #    9: indices
# #   10: arctan2
# #   11: arange
# #   12: height
# #   13: width
# #   14: linspace
# #   15: int32
# #   16: color_buffer
# #   17: color_buffer_flag
# # Varnames:
# #	self, effect, speed, colors, color1, color2, color3, color4, buf_w, buf_h, cx, cy, radius, reversed_spin, buf, offset, y_grid, x_grid, dx, dy, angles, dist_sq, in_circle, i, start, end, s_angle, e_angle, c, mask, h_scr, w_scr, x_map, y_map
# # Positional arguments:
# #	self
# # Local variables:
# #    1: effect
# #    2: speed
# #    3: colors
# #    4: color1
# #    5: color2
# #    6: color3
# #    7: color4
# #    8: buf_w
# #    9: buf_h
# #   10: cx
# #   11: cy
# #   12: radius
# #   13: reversed_spin
# #   14: buf
# #   15: offset
# #   16: y_grid
# #   17: x_grid
# #   18: dx
# #   19: dy
# #   20: angles
# #   21: dist_sq
# #   22: in_circle
# #   23: i
# #   24: start
# #   25: end
# #   26: s_angle
# #   27: e_angle
# #   28: c
# #   29: mask
# #   30: h_scr
# #   31: w_scr
# #   32: x_map
# #   33: y_map
# 
# # Method Name:       _update_spiral_effect
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  34
# # Stack size:        6
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        6715
# # Constants:
# #    0: None
# #    1: 'spiral'
# #    2: 'speed'
# #    3: 0.9
# #    4: 'colors'
# #    5: ('dtype',)
# #    6: 0
# #    7: 1
# #    8: 2
# #    9: 3
# #   10: 100
# #   11: 50
# #   12: False
# #   13: 'spiral_buf_luoxuan'
# #   14: 0.0
# #   15: 360.0
# #   16: 360
# #   17: 0.5
# #   18: 1.0
# #   19: 45
# #   20: 180
# #   21: 225
# #   22: 90
# #   23: 270
# #   24: 135
# #   25: 315
# # Names:
# #    0: effects
# #    1: np
# #    2: array
# #    3: uint8
# #    4: hasattr
# #    5: zeros
# #    6: spiral_buf_luoxuan
# #    7: phase_spiral_effect
# #    8: pi
# #    9: indices
# #   10: arctan2
# #   11: arange
# #   12: height
# #   13: width
# #   14: linspace
# #   15: int32
# #   16: color_buffer
# #   17: color_buffer_flag
# # Varnames:
# #	self, effect, speed, colors, color1, color2, color3, color4, buf_w, buf_h, cx, cy, radius, reversed_spin, buf, offset, y_grid, x_grid, dx, dy, angles, dist_sq, in_circle, i, start, end, s_angle, e_angle, c, mask, h_scr, w_scr, x_map, y_map
# # Positional arguments:
# #	self
# # Local variables:
# #    1: effect
# #    2: speed
# #    3: colors
# #    4: color1
# #    5: color2
# #    6: color3
# #    7: color4
# #    8: buf_w
# #    9: buf_h
# #   10: cx
# #   11: cy
# #   12: radius
# #   13: reversed_spin
# #   14: buf
# #   15: offset
# #   16: y_grid
# #   17: x_grid
# #   18: dx
# #   19: dy
# #   20: angles
# #   21: dist_sq
# #   22: in_circle
# #   23: i
# #   24: start
# #   25: end
# #   26: s_angle
# #   27: e_angle
# #   28: c
# #   29: mask
# #   30: h_scr
# #   31: w_scr
# #   32: x_map
# #   33: y_map
# 6716:           0 LOAD_FAST            (self)
#                2 LOAD_ATTR            (effects)
#                4 LOAD_CONST           ("spiral")
#                6 BINARY_SUBSCR
#                8 STORE_FAST           (effect)
# 
# 6717:          10 LOAD_FAST            (effect)
#               12 LOAD_CONST           ("speed")
#               14 BINARY_SUBSCR
#               16 LOAD_CONST           (0.9)
#               18 BINARY_MULTIPLY
#               20 STORE_FAST           (speed)
# 
# 6719:          22 LOAD_GLOBAL          (np)
#               24 LOAD_ATTR            (array)
#               26 LOAD_FAST            (effect)
#               28 LOAD_CONST           ("colors")
#               30 BINARY_SUBSCR
#               32 LOAD_GLOBAL          (np)
#               34 LOAD_ATTR            (uint8)
#               36 LOAD_CONST           (('dtype',))
#               38 CALL_FUNCTION_KW     (2 total positional and keyword args)
#               40 STORE_FAST           (colors)
# 
# 6720:          42 LOAD_FAST            (colors)
#               44 LOAD_CONST           (0)
#               46 BINARY_SUBSCR
#               48 STORE_FAST           (color1)
# 
# 6721:          50 LOAD_FAST            (colors)
#               52 LOAD_CONST           (1)
#               54 BINARY_SUBSCR
#               56 STORE_FAST           (color2)
# 
# 6722:          58 LOAD_FAST            (colors)
#               60 LOAD_CONST           (2)
#               62 BINARY_SUBSCR
#               64 STORE_FAST           (color3)
# 
# 6723:          66 LOAD_FAST            (colors)
#               68 LOAD_CONST           (3)
#               70 BINARY_SUBSCR
#               72 STORE_FAST           (color4)
# 
# 6728:          74 LOAD_CONST           (100)
#               76 STORE_FAST           (buf_w)
# 
# 6729:          78 LOAD_CONST           (100)
#               80 STORE_FAST           (buf_h)
# 
# 6730:          82 LOAD_CONST           (50)
#               84 STORE_FAST           (cx)
# 
# 6731:          86 LOAD_CONST           (50)
#               88 STORE_FAST           (cy)
# 
# 6732:          90 LOAD_CONST           (100)
#               92 STORE_FAST           (radius)
# 
# 6733:          94 LOAD_CONST           (False)
#               96 STORE_FAST           (reversed_spin)
# 
# 6736:          98 LOAD_GLOBAL          (hasattr)
#              100 LOAD_FAST            (self)
#              102 LOAD_CONST           ("spiral_buf_luoxuan")
#              104 CALL_FUNCTION        (2 positional arguments)
#              106 POP_JUMP_IF_TRUE     (to 138)
# 
# 6737:         108 LOAD_GLOBAL          (np)
#              110 LOAD_ATTR            (zeros)
#              112 LOAD_FAST            (buf_h)
#              114 LOAD_FAST            (buf_w)
#              116 LOAD_CONST           (3)
#              118 BUILD_TUPLE          3
#              120 LOAD_GLOBAL          (np)
#              122 LOAD_ATTR            (uint8)
#              124 LOAD_CONST           (('dtype',))
#              126 CALL_FUNCTION_KW     (2 total positional and keyword args)
#              128 LOAD_FAST            (self)
#              130 STORE_ATTR           (spiral_buf_luoxuan)
# 
# 6738:         132 LOAD_CONST           (0.0)
#              134 LOAD_FAST            (self)
#              136 STORE_ATTR           (phase_spiral_effect)
# 
# 6740:     >>  138 LOAD_FAST            (self)
#              140 LOAD_ATTR            (spiral_buf_luoxuan)
#              142 STORE_FAST           (buf)
# 
# 6741:         144 LOAD_CONST           (0)
#              146 LOAD_FAST            (buf)
#              148 LOAD_CONST           (None)
#              150 LOAD_CONST           (None)
#              152 BUILD_SLICE          2
#              154 STORE_SUBSCR
# 
# 6744:         156 LOAD_FAST            (reversed_spin)
#              158 POP_JUMP_IF_FALSE    (to 176)
# 
# 6745:         160 LOAD_FAST            (self)
#              162 DUP_TOP
#              164 LOAD_ATTR            (phase_spiral_effect)
#              166 LOAD_FAST            (speed)
#              168 INPLACE_SUBTRACT
#              170 ROT_TWO
#              172 STORE_ATTR           (phase_spiral_effect)
#              174 JUMP_FORWARD         (to 190)
# 
# 6747:     >>  176 LOAD_FAST            (self)
#              178 DUP_TOP
#              180 LOAD_ATTR            (phase_spiral_effect)
#              182 LOAD_FAST            (speed)
#              184 INPLACE_ADD
#              186 ROT_TWO
#              188 STORE_ATTR           (phase_spiral_effect)
# 
# 6749:     >>  190 LOAD_CONST           (2)
#              192 LOAD_GLOBAL          (np)
#              194 LOAD_ATTR            (pi)
#              196 BINARY_MULTIPLY
#              198 LOAD_FAST            (self)
#              200 LOAD_ATTR            (phase_spiral_effect)
#              202 LOAD_CONST           (360.0)
#              204 BINARY_TRUE_DIVIDE
#              206 BINARY_MULTIPLY
#              208 STORE_FAST           (offset)
# 
# 6752:         210 LOAD_GLOBAL          (np)
#              212 LOAD_METHOD          (indices)
#              214 LOAD_FAST            (buf_h)
#              216 LOAD_FAST            (buf_w)
#              218 BUILD_TUPLE          2
#              220 CALL_METHOD          (1 positional argument)
#              222 UNPACK_SEQUENCE      2
#              224 STORE_FAST           (y_grid)
#              226 STORE_FAST           (x_grid)
# 
# 6753:         228 LOAD_FAST            (x_grid)
#              230 LOAD_FAST            (cx)
#              232 BINARY_SUBTRACT
#              234 STORE_FAST           (dx)
# 
# 6754:         236 LOAD_FAST            (y_grid)
#              238 LOAD_FAST            (cy)
#              240 BINARY_SUBTRACT
#              242 STORE_FAST           (dy)
# 
# 6757:         244 LOAD_GLOBAL          (np)
#              246 LOAD_METHOD          (arctan2)
#              248 LOAD_FAST            (dy)
#              250 LOAD_FAST            (dx)
#              252 CALL_METHOD          (2 positional arguments)
#              254 LOAD_GLOBAL          (np)
#              256 LOAD_ATTR            (pi)
#              258 BINARY_ADD
#              260 STORE_FAST           (angles)
# 
# 6758:         262 LOAD_FAST            (dx)
#              264 LOAD_CONST           (2)
#              266 BINARY_POWER
#              268 LOAD_FAST            (dy)
#              270 LOAD_CONST           (2)
#              272 BINARY_POWER
#              274 BINARY_ADD
#              276 STORE_FAST           (dist_sq)
# 
# 6759:         278 LOAD_FAST            (dist_sq)
#              280 LOAD_FAST            (radius)
#              282 LOAD_CONST           (2)
#              284 BINARY_POWER
#              286 COMPARE_OP           (<=)
#              288 STORE_FAST           (in_circle)
# 
# 6762:         290 LOAD_GLOBAL          (np)
#              292 LOAD_METHOD          (arange)
#              294 LOAD_CONST           (0)
#              296 LOAD_CONST           (360)
#              298 LOAD_CONST           (0.5)
#              300 CALL_METHOD          (3 positional arguments)
#              302 GET_ITER
#          >>  304 EXTENDED_ARG         (256)
#              306 FOR_ITER             (to 678)
#              308 STORE_FAST           (i)
# 
# 6763:         310 LOAD_CONST           (2)
#              312 LOAD_GLOBAL          (np)
#              314 LOAD_ATTR            (pi)
#              316 BINARY_MULTIPLY
#              318 LOAD_FAST            (i)
#              320 LOAD_CONST           (360.0)
#              322 BINARY_TRUE_DIVIDE
#              324 BINARY_MULTIPLY
#              326 STORE_FAST           (start)
# 
# 6764:         328 LOAD_CONST           (2)
#              330 LOAD_GLOBAL          (np)
#              332 LOAD_ATTR            (pi)
#              334 BINARY_MULTIPLY
#              336 LOAD_FAST            (i)
#              338 LOAD_CONST           (1.0)
#              340 BINARY_ADD
#              342 LOAD_CONST           (360.0)
#              344 BINARY_TRUE_DIVIDE
#              346 BINARY_MULTIPLY
#              348 STORE_FAST           (end)
# 
# 6766:         350 LOAD_FAST            (start)
#              352 LOAD_FAST            (offset)
#              354 BINARY_ADD
#              356 LOAD_CONST           (2)
#              358 LOAD_GLOBAL          (np)
#              360 LOAD_ATTR            (pi)
#              362 BINARY_MULTIPLY
#              364 BINARY_MODULO
#              366 STORE_FAST           (s_angle)
# 
# 6767:         368 LOAD_FAST            (end)
#              370 LOAD_FAST            (offset)
#              372 BINARY_ADD
#              374 LOAD_CONST           (2)
#              376 LOAD_GLOBAL          (np)
#              378 LOAD_ATTR            (pi)
#              380 BINARY_MULTIPLY
#              382 BINARY_MODULO
#              384 STORE_FAST           (e_angle)
# 
# 6770:         386 LOAD_FAST            (i)
#              388 LOAD_CONST           (45)
#              390 COMPARE_OP           (<)
#              392 EXTENDED_ARG         (256)
#              394 POP_JUMP_IF_TRUE     (to 424)
#              396 LOAD_CONST           (180)
#              398 LOAD_FAST            (i)
#              400 DUP_TOP
#              402 ROT_THREE
#              404 COMPARE_OP           (<)
#              406 EXTENDED_ARG         (256)
#              408 POP_JUMP_IF_FALSE    (to 420)
#              410 LOAD_CONST           (225)
#              412 COMPARE_OP           (<)
#              414 EXTENDED_ARG         (256)
#              416 POP_JUMP_IF_FALSE    (to 430)
#              418 JUMP_FORWARD         (to 424)
#          >>  420 POP_TOP
#              422 JUMP_FORWARD         (to 430)
# 
# 6771:     >>  424 LOAD_FAST            (color1)
#              426 STORE_FAST           (c)
#              428 JUMP_FORWARD         (to 614)
# 
# 6772:     >>  430 LOAD_CONST           (45)
#              432 LOAD_FAST            (i)
#              434 DUP_TOP
#              436 ROT_THREE
#              438 COMPARE_OP           (<)
#              440 EXTENDED_ARG         (256)
#              442 POP_JUMP_IF_FALSE    (to 454)
#              444 LOAD_CONST           (90)
#              446 COMPARE_OP           (<)
#              448 EXTENDED_ARG         (256)
#              450 POP_JUMP_IF_TRUE     (to 484)
#              452 JUMP_FORWARD         (to 456)
#          >>  454 POP_TOP
#          >>  456 LOAD_CONST           (225)
#              458 LOAD_FAST            (i)
#              460 DUP_TOP
#              462 ROT_THREE
#              464 COMPARE_OP           (<)
#              466 EXTENDED_ARG         (256)
#              468 POP_JUMP_IF_FALSE    (to 480)
#              470 LOAD_CONST           (270)
#              472 COMPARE_OP           (<)
#              474 EXTENDED_ARG         (256)
#              476 POP_JUMP_IF_FALSE    (to 490)
#              478 JUMP_FORWARD         (to 484)
#          >>  480 POP_TOP
#              482 JUMP_FORWARD         (to 490)
# 
# 6773:     >>  484 LOAD_FAST            (color2)
#              486 STORE_FAST           (c)
#              488 JUMP_FORWARD         (to 614)
# 
# 6774:     >>  490 LOAD_CONST           (90)
#              492 LOAD_FAST            (i)
#              494 DUP_TOP
#              496 ROT_THREE
#              498 COMPARE_OP           (<)
#              500 EXTENDED_ARG         (512)
#              502 POP_JUMP_IF_FALSE    (to 514)
#              504 LOAD_CONST           (135)
#              506 COMPARE_OP           (<)
#              508 EXTENDED_ARG         (512)
#              510 POP_JUMP_IF_TRUE     (to 544)
#              512 JUMP_FORWARD         (to 516)
#          >>  514 POP_TOP
#          >>  516 LOAD_CONST           (270)
#              518 LOAD_FAST            (i)
#              520 DUP_TOP
#              522 ROT_THREE
#              524 COMPARE_OP           (<)
#              526 EXTENDED_ARG         (512)
#              528 POP_JUMP_IF_FALSE    (to 540)
#              530 LOAD_CONST           (315)
#              532 COMPARE_OP           (<)
#              534 EXTENDED_ARG         (512)
#              536 POP_JUMP_IF_FALSE    (to 550)
#              538 JUMP_FORWARD         (to 544)
#          >>  540 POP_TOP
#              542 JUMP_FORWARD         (to 550)
# 
# 6775:     >>  544 LOAD_FAST            (color3)
#              546 STORE_FAST           (c)
#              548 JUMP_FORWARD         (to 614)
# 
# 6776:     >>  550 LOAD_CONST           (135)
#              552 LOAD_FAST            (i)
#              554 DUP_TOP
#              556 ROT_THREE
#              558 COMPARE_OP           (<)
#              560 EXTENDED_ARG         (512)
#              562 POP_JUMP_IF_FALSE    (to 574)
#              564 LOAD_CONST           (180)
#              566 COMPARE_OP           (<)
#              568 EXTENDED_ARG         (512)
#              570 POP_JUMP_IF_TRUE     (to 604)
#              572 JUMP_FORWARD         (to 576)
#          >>  574 POP_TOP
#          >>  576 LOAD_CONST           (315)
#              578 LOAD_FAST            (i)
#              580 DUP_TOP
#              582 ROT_THREE
#              584 COMPARE_OP           (<)
#              586 EXTENDED_ARG         (512)
#              588 POP_JUMP_IF_FALSE    (to 600)
#              590 LOAD_CONST           (360)
#              592 COMPARE_OP           (<)
#              594 EXTENDED_ARG         (256)
#              596 POP_JUMP_IF_FALSE    (to 304)
#              598 JUMP_FORWARD         (to 604)
#          >>  600 POP_TOP
#              602 JUMP_FORWARD         (to 610)
# 
# 6777:     >>  604 LOAD_FAST            (color4)
#              606 STORE_FAST           (c)
#              608 JUMP_FORWARD         (to 614)
# 
# 6779:     >>  610 EXTENDED_ARG         (256)
#              612 JUMP_ABSOLUTE        (to 304)
# 
# 6781:     >>  614 LOAD_FAST            (s_angle)
#              616 LOAD_FAST            (e_angle)
#              618 COMPARE_OP           (<)
#              620 EXTENDED_ARG         (512)
#              622 POP_JUMP_IF_FALSE    (to 646)
# 
# 6782:         624 LOAD_FAST            (angles)
#              626 LOAD_FAST            (s_angle)
#              628 COMPARE_OP           (>=)
#              630 LOAD_FAST            (angles)
#              632 LOAD_FAST            (e_angle)
#              634 COMPARE_OP           (<)
#              636 BINARY_AND
#              638 LOAD_FAST            (in_circle)
#              640 BINARY_AND
#              642 STORE_FAST           (mask)
#              644 JUMP_FORWARD         (to 666)
# 
# 6784:     >>  646 LOAD_FAST            (angles)
#              648 LOAD_FAST            (s_angle)
#              650 COMPARE_OP           (>=)
#              652 LOAD_FAST            (angles)
#              654 LOAD_FAST            (e_angle)
#              656 COMPARE_OP           (<)
#              658 BINARY_OR
#              660 LOAD_FAST            (in_circle)
#              662 BINARY_AND
#              664 STORE_FAST           (mask)
# 
# 6786:     >>  666 LOAD_FAST            (c)
#              668 LOAD_FAST            (buf)
#              670 LOAD_FAST            (mask)
#              672 STORE_SUBSCR
#              674 EXTENDED_ARG         (256)
#              676 JUMP_ABSOLUTE        (to 304)
# 
# 6789:     >>  678 LOAD_FAST            (self)
#              680 LOAD_ATTR            (height)
#              682 LOAD_FAST            (self)
#              684 LOAD_ATTR            (width)
#              686 ROT_TWO
#              688 STORE_FAST           (h_scr)
#              690 STORE_FAST           (w_scr)
# 
# 6790:         692 LOAD_GLOBAL          (np)
#              694 LOAD_ATTR            (linspace)
#              696 LOAD_CONST           (0)
#              698 LOAD_FAST            (buf_w)
#              700 LOAD_CONST           (1)
#              702 BINARY_SUBTRACT
#              704 LOAD_FAST            (w_scr)
#              706 LOAD_GLOBAL          (np)
#              708 LOAD_ATTR            (int32)
#              710 LOAD_CONST           (('dtype',))
#              712 CALL_FUNCTION_KW     (4 total positional and keyword args)
#              714 STORE_FAST           (x_map)
# 
# 6791:         716 LOAD_GLOBAL          (np)
#              718 LOAD_ATTR            (linspace)
#              720 LOAD_CONST           (0)
#              722 LOAD_FAST            (buf_h)
#              724 LOAD_CONST           (1)
#              726 BINARY_SUBTRACT
#              728 LOAD_FAST            (h_scr)
#              730 LOAD_GLOBAL          (np)
#              732 LOAD_ATTR            (int32)
#              734 LOAD_CONST           (('dtype',))
#              736 CALL_FUNCTION_KW     (4 total positional and keyword args)
#              738 STORE_FAST           (y_map)
# 
# 6792:         740 LOAD_FAST            (buf)
#              742 LOAD_FAST            (y_map)
#              744 LOAD_CONST           (None)
#              746 LOAD_CONST           (None)
#              748 BUILD_SLICE          2
#              750 LOAD_CONST           (None)
#              752 BUILD_TUPLE          2
#              754 BINARY_SUBSCR
#              756 LOAD_FAST            (x_map)
#              758 BUILD_TUPLE          2
#              760 BINARY_SUBSCR
#              762 LOAD_FAST            (self)
#              764 LOAD_ATTR            (color_buffer)
#              766 LOAD_CONST           (None)
#              768 LOAD_CONST           (None)
#              770 BUILD_SLICE          2
#              772 STORE_SUBSCR
# 
# 6793:         774 LOAD_CONST           (1)
#              776 LOAD_FAST            (self)
#              778 STORE_ATTR           (color_buffer_flag)
#              780 LOAD_CONST           (None)
#              782 RETURN_VALUE
# 
# 
# ==== .LightEffectManager._update_poison_effect (linha 6801)
effect = self.effects["poison"]
speed = max(1, effect["speed"])
speed_scale = speed / 10.0
colors = np.array((effect["colors"]), dtype=(np.uint8))
bgColor = colors[0]
color1 = colors[1]
color2 = colors[2]
color3 = colors[3]
buf_w = 100
buf_h = 100
max_particles = 4
if not hasattr(self, "poison_buf_duyao"):
    self.poison_buf_duyao = np.full((buf_h, buf_w, 3), bgColor, np.uint8)
    self.particles_duyao = []
buf = self.poison_buf_duyao
buf = np.where(buf != bgColor, buf * 0.85 + bgColor * 0.15, buf).astype(np.uint8)
while len(self.particles_duyao) < max_particles * 2:
    if np.random.random() > 0.5:
        p = {'x':(np.random.uniform)(0, buf_w),  'y':(np.random.uniform)(-50, -10), 
         'speedX':(np.random.uniform(0, 1)) * (np.random.choice([-1, 1])), 
         'speedY':(np.random.uniform)(0.5, 2.5), 
         'size':(np.random.randint)(1, 11), 
         'radius':1.0, 
         'color_idx':(np.random.randint)(0, 3), 
         'dir':"down"}
    else:
        p = {'x':(np.random.uniform)(0, buf_w),  'y':buf_h + (np.random.uniform(50, 150)), 
         'speedX':(np.random.uniform(0, 1)) * (np.random.choice([-1, 1])), 
         'speedY':(np.random.uniform)(0.5, 2.5), 
         'size':(np.random.randint)(1, 11), 
         'radius':1.0, 
         'color_idx':(np.random.randint)(0, 3), 
         'dir':"up"}
    self.particles_duyao.append(p)

alive = []
for p in self.particles_duyao:
    cidx = p["color_idx"]
    if cidx == 0:
        col = color1
    elif cidx == 1:
        col = color2
    else:
        col = color3
    x = p["x"]
    y = p["y"]
    r = p["radius"]
    w = p["size"]
    cx = int(x)
    cy = int(y)
    hw = w // 2
    ir = max(0, int(r - hw))
    or_ = int(r + hw)
    if or_ > 0:
        yp, xp = np.indices((buf_h, buf_w))
        dx = xp - cx
        dy = yp - cy
        d2 = dx * dx + dy * dy
        mask = (d2 >= ir ** 2) & (d2 <= or_ ** 2)
        buf[mask] = col
    p["x"] += p["speedX"] * speed_scale
    if p["dir"] == "down":
        p["y"] += p["speedY"] * speed_scale
    else:
        p["y"] -= p["speedY"] * speed_scale
    p["radius"] += np.random.random() / 1.3 * speed_scale
    if p["dir"] == "down":
        if p["y"] < buf_h + 50:
            alive.append(p)
        if p["dir"] == "up":
            if p["y"] > -50:
                alive.append(p)
            self.particles_duyao = alive
            h_scr, w_scr = self.height, self.width
            x_map = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
            y_map = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
            self.color_buffer[:] = buf[(y_map[:, None], x_map)]
            self.color_buffer_flag = 1
# ==== .LightEffectManager._update_windmill_effect (linha 6916)
effect = self.effects["windmill"]
speed = max(1, min(10, effect["speed"]))
speed_scale = speed * 0.35
scale_speed = speed * 0.12
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
pin_color = colors[1]
buf_w = 100
buf_h = 100
cx = 50
cy = 50
blade_len = 50
blade_wid = 12
center_r = 5
direction = 1
if not hasattr(self, "windmill_buf"):
    self.windmill_buf = np.full((buf_h, buf_w, 3), bg_color, np.uint8)
    self.angle = 0.0
    self.scale_phase = 0.0
buf = self.windmill_buf
buf[:] = bg_color
self.angle += direction * speed_scale
self.angle %= 360
rad = np.radians(self.angle)
self.scale_phase += scale_speed
self.scale_phase %= 360
scale = 0.6 + 0.4 * np.sin(np.radians(self.scale_phase))
current_len = blade_len * scale
current_wid = blade_wid * scale
y_grid, x_grid = np.indices((buf_h, buf_w))
dx = x_grid - cx
dy = y_grid - cy
for i in range(4):
    a = rad + i * np.pi / 2
    ca = np.cos(a)
    sa = np.sin(a)
    x1 = cx + ca * current_len - sa * current_wid
    y1 = cy + sa * current_len + ca * current_wid
    x2 = cx + ca * current_len + sa * current_wid
    y2 = cy + sa * current_len - ca * current_wid
    x3 = cx
    y3 = cy
    x4 = cx
    y4 = cy
    pts = np.array([(x1, y1), (x2, y2), (x3, y3), (x4, y4)])
    n = len(pts)
    inside = np.zeros_like(dx, dtype=bool)
    for j in range(n):
        xj, yj = pts[j]
        xk, yk = pts[(j + 1) % n]
        mask = ((yj > y_grid) != (yk > y_grid)) & (dx < (xk - xj) * (y_grid - yj) / (yk - yj + 1e-08) + (xj - cx))
        inside ^= mask

    buf[inside] = pin_color

dist_c = dx ** 2 + dy ** 2
buf[dist_c <= (center_r * scale) ** 2] = pin_color
h_scr, w_scr = self.height, self.width
x_map = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
y_map = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
self.color_buffer[:] = buf[(y_map[:, None], x_map)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_maple_leaf_effect (linha 7019)
effect = self.effects["maple_leaf"]
speed = np.clip(effect["speed"], 1, 10)
fall_speed = speed * 0.18
rotate_speed = speed * 0.04
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg = colors[0]
c1 = colors[1]
c2 = colors[2]
c3 = colors[3]
buf_w = 100
buf_h = 100
max_leaves = 10
leaf_scale = 0.9
if not hasattr(self, "maple_buf"):
    self.maple_buf = np.full((buf_h, buf_w, 3), bg, dtype=(np.uint8))
    self.leaves = []
    self.leaf_pattern = np.array([
     (0, -30), (5, -25), (8, -18), (6, -10), (2, -3),
     (-3, 0), (-8, 4), (-12, 10), (-10, 18), (-5, 25),
     (0, 30), (5, 25), (10, 18), (12, 10), (8, 0),
     (3, -6), (0, -12), (-3, -18), (-5, -25)])
buf = self.maple_buf
buf[:] = bg
while len(self.leaves) < max_leaves:
    color_idx = np.random.randint(0, 3)
    leaf = {'x':(np.random.uniform)(0, buf_w), 
     'y':(np.random.uniform)(-40, -10), 
     'rot':(np.random.uniform)(0, 360), 
     'rot_dir':(np.random.choice)([-1, 1]), 
     'color_idx':color_idx}
    self.leaves.append(leaf)

alive = []
lp = self.leaf_pattern * leaf_scale
y_indices, x_indices = np.indices((buf_h, buf_w))
for leaf in self.leaves:
    x = leaf["x"]
    y = leaf["y"]
    rot = leaf["rot"]
    cidx = leaf["color_idx"]
    if cidx == 0:
        col = c1
    elif cidx == 1:
        col = c2
    else:
        col = c3
    rad = np.radians(rot)
    cos_r = np.cos(rad)
    sin_r = np.sin(rad)
    x_rot = lp[:, 0] * cos_r - lp[:, 1] * sin_r + x
    y_rot = lp[:, 0] * sin_r + lp[:, 1] * cos_r + y
    inside = np.zeros((buf_h, buf_w), dtype=bool)
    n = len(x_rot)
    for i in range(n):
        j = (i + 1) % n
        xi1, yi1 = x_rot[i], y_rot[i]
        xi2, yi2 = x_rot[j], y_rot[j]
        dy = yi2 - yi1
        if abs(dy) < 1e-08:
            pass
        else:
            t = (y_indices - yi1) / dy
            x_intersect = xi1 + t * (xi2 - xi1)
            mask = ((yi1 > y_indices) != (yi2 > y_indices)) & (x_indices < x_intersect)
            inside ^= mask
        buf[inside] = col
        leaf["y"] += fall_speed
        leaf["rot"] += leaf["rot_dir"] * rotate_speed

    if leaf["rot"] > 360:
        leaf["rot"] -= 360
    elif leaf["rot"] < 0:
        leaf["rot"] += 360
    if leaf["y"] < buf_h + 30:
        alive.append(leaf)
    self.leaves = alive
    h, w = self.height, self.width
    x_map = np.linspace(0, (buf_w - 1), w, dtype=(np.int32))
    y_map = np.linspace(0, (buf_h - 1), h, dtype=(np.int32))
    self.color_buffer[:] = buf[(y_map[:, None], x_map)]
    self.color_buffer_flag = 1
# ==== .LightEffectManager._update_lightsaber_battle_effect (linha 7132)
effect = self.effects["lightsaber_battle"]
speed = np.clip(effect["speed"], 1, 10)
anim_speed = speed * 0.1
colors = np.array((effect["colors"]), dtype=(np.uint8))
c1 = colors[0]
c2 = colors[1]
bg = [0, 0, 0]
buf_w = 100
buf_h = 100
base_len = 42
blade_w = 4
tip_r = 5
if not hasattr(self, "ls_buf"):
    self.ls_buf = np.full((buf_h, buf_w, 3), bg, np.uint8)
    self.frame = 0
    self.state = 0
    self.y1 = np.random.uniform(20, 80)
    self.y2 = np.random.uniform(20, 80)
    self.win_saber = np.random.randint(0, 2)
buf = self.ls_buf
buf[:] = bg
yy, xx = np.indices((buf_h, buf_w), dtype=(np.float32))
self.frame += anim_speed
f = self.frame
tx, ty = (50, 50)
if self.state == 0:
    t = np.clip(f / 30, 0, 1)
    x1 = 2 + t * (tx - 2)
    y1 = self.y1 + t * (ty - self.y1)
    dir1 = np.arctan2(ty - y1, tx - x1)
    x2 = 98 - t * (98 - tx)
    y2 = self.y2 + t * (ty - self.y2)
    dir2 = np.arctan2(ty - y2, tx - x2)
    if t >= 1:
        self.state = 1
        self.frame = 0
elif self.state == 1:
    x1 = tx
    y1 = ty
    dir1 = np.random.uniform(-0.6, 0.6)
    x2 = tx
    y2 = ty
    dir2 = np.random.uniform(-0.6, 0.6)
    if f > 10:
        self.win_saber = np.random.randint(0, 2)
        self.state = 2
        self.frame = 0
elif self.state == 2:
    t = np.clip(f / 35, 0, 1)
    x1 = x2 = tx
    y1 = y2 = ty
    dir1 = dir2 = t * 9.0
    base_len = 42 * (1 + t * 4.5)
    if t >= 1:
        self.state = 3
        self.frame = 0
elif self.state == 3:
    t = np.clip(f / 20, 0, 1)
    x1 = x2 = tx
    y1 = y2 = ty
    dir1 = dir2 = f * 0.12
    base_len = 210.0
    if t >= 1:
        self.state = 0
        self.frame = 0
        self.y1 = np.random.uniform(20, 80)
        self.y2 = np.random.uniform(20, 80)

def draw_saber(x, y, ang, col):
    ca = np.cos(ang)
    sa = np.sin(ang)
    x0 = x - sa * blade_w
    y0 = y + ca * blade_w
    x1s = x + sa * blade_w
    y1s = y - ca * blade_w
    x2s = x + ca * base_len + sa * blade_w
    y2s = y - sa * base_len - ca * blade_w
    x3s = x + ca * base_len - sa * blade_w
    y3s = y - sa * base_len + ca * blade_w
    pts = [(x0, y0), (x1s, y1s), (x2s, y2s), (x3s, y3s)]
    inside = np.zeros((buf_h, buf_w), bool)
    for i in range(4):
        sx, sy = pts[i]
        ex, ey = pts[(i + 1) % 4]
        dy = ey - sy
        if abs(dy) < 1e-08:
            pass
        else:
            t = (yy - sy) / dy
            ix = sx + t * (ex - sx)
            mask = ((sy > yy) != (ey > yy)) & (xx < ix)
            inside ^= mask
        buf[inside] = col
        tx_p = x + ca * base_len
        ty_p = y - sa * base_len
        buf[(xx - tx_p) ** 2 + (yy - ty_p) ** 2 <= tip_r ** 2] = col


if self.state < 2:
    draw_saber(x1, y1, dir1, c1)
    draw_saber(x2, y2, dir2, c2)
elif self.win_saber == 0:
    draw_saber(x1, y1, dir1, c1)
else:
    draw_saber(x2, y2, dir2, c2)
h, w = self.height, self.width
xmap = np.linspace(0, 99, w, dtype=(np.int32))
ymap = np.linspace(0, 99, h, dtype=(np.int32))
self.color_buffer[:] = buf[(ymap[:, None], xmap)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_lightsaber_battle_effect.draw_saber (linha 7230)
ca = np.cos(ang)
sa = np.sin(ang)
x0 = x - sa * blade_w
y0 = y + ca * blade_w
x1s = x + sa * blade_w
y1s = y - ca * blade_w
x2s = x + ca * base_len + sa * blade_w
y2s = y - sa * base_len - ca * blade_w
x3s = x + ca * base_len - sa * blade_w
y3s = y - sa * base_len + ca * blade_w
pts = [(x0, y0), (x1s, y1s), (x2s, y2s), (x3s, y3s)]
inside = np.zeros((buf_h, buf_w), bool)
for i in range(4):
    sx, sy = pts[i]
    ex, ey = pts[(i + 1) % 4]
    dy = ey - sy
    if abs(dy) < 1e-08:
        pass
    else:
        t = (yy - sy) / dy
        ix = sx + t * (ex - sx)
        mask = ((sy > yy) != (ey > yy)) & (xx < ix)
        inside ^= mask
    buf[inside] = col
    tx_p = x + ca * base_len
    ty_p = y - sa * base_len
    buf[(xx - tx_p) ** 2 + (yy - ty_p) ** 2 <= tip_r ** 2] = col
# ==== .LightEffectManager._update_trace_growth_effect (linha 7280)
effect = self.effects["trace_growth"]
speed = np.clip(effect["speed"], 1, 10)
anim_speed = speed * 0.12
colors = np.array((effect["colors"]), dtype=(np.uint8))
fg_color = colors[0]
bg_color = np.array([0, 0, 0], dtype=(np.uint8))
buf_w = 100
buf_h = 100
min_dist = 4
max_dist = 30
init_width = 12
max_lines = 60
init_lines = 4
fade_alpha = 0.12
dirs = np.array([[0, 1], [1, 0], [0, -1], [-1, 0],
 [
  0.7, 0.7], [0.7, -0.7], [-0.7, 0.7], [-0.7, -0.7]])
if not hasattr(self, "trace_buf"):
    self.trace_buf = np.full((buf_h, buf_w, 3), bg_color, dtype=(np.uint8))
    self.trace_frame = 0
    self.hue_pos = 0
    self.lines = []
    cx, cy = buf_w // 2, buf_h // 2
    for _ in range(init_lines):
        self.lines.append([cx,cy,0.0,0.0,init_width,0.0])

buf = self.trace_buf
yy, xx = np.indices((buf_h, buf_w), dtype=(np.float32))
self.trace_frame += 1
self.hue_pos = (self.hue_pos + 1) % 360
buf = (buf * (1 - fade_alpha) + bg_color * fade_alpha).astype(np.uint8)
new_lines = []
dead_lines = []
for i, line in enumerate(self.lines):
    x, y, vx, vy, width, dist = line
    prev_x, prev_y = x, y
    x += vx * anim_speed
    y += vy * anim_speed
    dist -= 1
    dead = False
    if x < 0 or x >= buf_w or x >= buf_w or y >= buf_h:
        dead = True
    if dist <= 0:
        if width > 1:
            if not not dead:
                dist = np.random.uniform(min_dist, max_dist)
                width /= 1.25
            d = dirs[np.random.randint(len(dirs))]
            new_vx, new_vy = d[0], d[1]
            if abs(new_vx + vx) < 0.1:
                if not abs(new_vy + vy) < 0.1:
                    break
                if len(self.lines) + len(new_lines) < max_lines:
                    new_lines.append([x,y,new_vx,new_vy,width,dist])
                if np.random.random() < 0.2:
                    dead = True
                if not dead:
                    dx = xx - x
                    dy = yy - y
                    dx_prev = xx - prev_x
                    dy_prev = yy - prev_y
                    line_mask = np.abs(dy * (prev_x - x) - dx * (prev_y - y)) < width / 2
                    line_mask &= np.hypot(xx - prev_x, yy - prev_y) + np.hypot(xx - x, yy - y) <= np.hypot(x - prev_x, y - prev_y) + width / 2
                    r = int(np.sin(self.hue_pos / 57.2958) * 127 + 128)
                    g = int(np.sin((self.hue_pos + 120) / 57.2958) * 127 + 128)
                    b = int(np.sin((self.hue_pos + 240) / 57.2958) * 127 + 128)
                    buf[line_mask] = [r, g, b]
                if not dead:
                    self.lines[i] = [
                     x,y,vx,vy,width,dist]
                else:
                    dead_lines.append(i)

for i in reversed(dead_lines):
    del self.lines[i]

self.lines.extend(new_lines)
if len(self.lines) < max_lines:
    if np.random.random() < 0.3:
        cx, cy = buf_w // 2, buf_h // 2
        d = dirs[np.random.randint(len(dirs))]
        vx, vy = d[0], d[1]
        self.lines.append([cx, cy, vx, vy, init_width, np.random.uniform(min_dist, max_dist)])
h_scr, w_scr = self.height, self.width
x_map = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
y_map = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
self.color_buffer[:] = buf[(y_map[:, None], x_map)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_police_light_effect (linha 7430)
effect = self.effects["police_light"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.float32))
color1 = colors[0]
color2 = colors[1]
color3 = colors[2]
buf_w = 100
buf_h = 100
bg = np.array([0, 0, 0], dtype=(np.float32))
if not hasattr(self, "police_buf"):
    self.police_buf = np.full((buf_h, buf_w, 3), bg, dtype=(np.float32))
    self.phase = 0.0
buf = self.police_buf
buf[:] = bg
yy, xx = np.indices((buf_h, buf_w), dtype=(np.int32))
self.phase += speed * 0.04
t = (np.sin(self.phase) + 1) / 2.0
left = xx < 33
right = xx >= 67
middle = (xx >= 33) & (xx < 67)
buf[left] = (1 - t) * color1 + t * color3
buf[right] = (1 - t) * color3 + t * color1
buf[middle] = color2 * 0.25
buf = np.clip(buf, 0, 255).astype(np.uint8)
h_scr, w_scr = self.height, self.width
x_map = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
y_map = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
self.color_buffer[:] = buf[(y_map[:, None], x_map)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_sphere_3d_effect (linha 7483)
effect = self.effects["sphere_3d"]
speed = np.clip(effect["speed"], 1, 10) * 0.01
colors = np.array((effect["colors"]), dtype=(np.uint8))
buf_w = 100
buf_h = 100
cx = buf_w // 2
cy = buf_h // 2
layer_count = 24
line_width = 20
sphere_radius = 50
if not hasattr(self, "sphere_buf"):
    self.sphere_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.rot_x = 0.0
    self.rot_y = 0.0
buf = self.sphere_buf
buf[:] = 0
yy, xx = np.indices((buf_h, buf_w), dtype=(np.float32))
hw = line_width // 2
self.rot_x += speed
self.rot_y += speed * 0.7
for i in range(layer_count):
    lat = np.pi * (i / layer_count)
    r = sphere_radius * np.sin(lat)
    z = sphere_radius * np.cos(lat)
    rx = self.rot_x
    ry = self.rot_y
    y_rot = r * np.cos(rx) - z * np.sin(rx)
    z_rot = r * np.sin(rx) + z * np.cos(rx)
    x_rot = z_rot * np.sin(ry)
    z_final = z_rot * np.cos(ry)
    scale = 1.0 + z_final / 100.0
    x_pos = cx + x_rot * scale
    y_pos = cy + y_rot * scale
    radius = 2 + abs(r * scale * 0.3)
    color = colors[i % len(colors)]
    dx = xx - x_pos
    dy = yy - y_pos
    dist_sq = dx ** 2 + dy ** 2
    inner = (radius - hw) ** 2
    outer = (radius + hw) ** 2
    mask = (dist_sq >= inner) & (dist_sq <= outer)
    buf[mask] = color

h_scr, w_scr = self.height, self.width
x_map = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
y_map = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
self.color_buffer[:] = buf[(y_map[:, None], x_map)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_dream_dream_effect (linha 7565)
effect = self.effects["dream_dream"]
speed = np.clip(effect["speed"], 1, 10) * 0.25
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg = colors[0]
c1 = colors[1]
c2 = colors[2]
c3 = colors[3]
buf_w = 100
buf_h = 100
cx = buf_w // 2
cy = buf_h // 2
line_w = 1.3
total_points = 120
spiral_size = 1.8
if not hasattr(self, "dream_buf"):
    self.dream_buf = np.full((buf_h, buf_w, 3), bg, dtype=(np.uint8))
    self.time = 0.0
buf = self.dream_buf
buf[:] = bg
yy, xx = np.indices((buf_h, buf_w), dtype=(np.float32))
self.time += speed
angle_offset = self.time / -200
theta = np.linspace(0, 12 * np.pi, total_points)
x = cx + theta * np.cos(theta + angle_offset) * spiral_size
y = cy + theta * np.sin(theta + angle_offset) * spiral_size
part = total_points // 3
for i in range(total_points):
    px = x[i]
    py = y[i]
    if i < part:
        col = c1
    elif i < part * 2:
        col = c2
    else:
        col = c3
    dist_sq = (xx - px) ** 2 + (yy - py) ** 2
    mask = dist_sq < line_w ** 2
    buf[mask] = col

h_scr, w_scr = self.height, self.width
x_map = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
y_map = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
self.color_buffer[:] = buf[(y_map[:, None], x_map)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_neon_wave_effect (linha 7649)
effect = self.effects["neon_wave"]
speed = np.clip(effect["speed"], 1, 10) * 0.1
colors = np.array((effect["colors"]), dtype=(np.uint8))
buf_w = 100
buf_h = 100
repeat = 4
angle = 0.0
if not hasattr(self, "wave_buf"):
    self.wave_buf = np.zeros((buf_h, buf_w, 3), dtype=(np.uint8))
    self.offset = 0.0
buf = self.wave_buf
buf[:] = 0
self.offset += speed
yy, xx = np.indices((buf_h, buf_w), dtype=(np.float32))
x_pos = (xx + self.offset) / (buf_w / repeat)
x_pos = x_pos % 1.0
r = np.zeros_like(x_pos, dtype=(np.uint8))
g = np.zeros_like(x_pos, dtype=(np.uint8))
b = np.zeros_like(x_pos, dtype=(np.uint8))
c0 = (x_pos >= 0.0) & (x_pos < 0.25)
c1 = (x_pos >= 0.25) & (x_pos < 0.5)
c2 = (x_pos >= 0.5) & (x_pos < 0.75)
c3 = (x_pos >= 0.75) & (x_pos <= 1.0)
buf[c0] = colors[0]
buf[c1] = colors[1]
buf[c2] = colors[2]
buf[c3] = colors[3]
h_scr, w_scr = self.height, self.width
x_map = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
y_map = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
self.color_buffer[:] = buf[(y_map[:, None], x_map)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_fireflies_effect (linha 7726)
effect = self.effects["fireflies"]
speed = np.clip(effect["speed"], 1, 10) * 0.12
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
fly_colors = colors[1:4]
buf_w = 100
buf_h = 100
max_fireflies = 15
base_size = 2
glow_size = 5
max_life = 80
if not hasattr(self, "firefly_buf"):
    self.firefly_buf = np.full((buf_h, buf_w, 3), bg_color, dtype=(np.uint8))
    self.fireflies = []
buf = self.firefly_buf
buf[:] = bg_color
yy, xx = np.indices((buf_h, buf_w), dtype=(np.float32))
while len(self.fireflies) < max_fireflies:
    life = np.random.uniform(max_life * 0.5, max_life)
    self.fireflies.append({'x':(np.random.uniform)(0, buf_w), 
     'y':(np.random.uniform)(0, buf_h), 
     'vx':(np.random.uniform)(-0.3, 0.3), 
     'vy':(np.random.uniform)(-0.3, 0.3), 
     'life':life, 
     'max_life':life, 
     'color':fly_colors[np.random.randint(3)]})

alive = []
for f in self.fireflies:
    f["life"] -= speed
    if f["life"] < 0:
        pass
    else:
        half = f["max_life"] / 2
        alpha = 1.0 - abs(half - f["life"]) / f["max_life"]
        alpha = np.clip(alpha, 0, 1)
        f["x"] += f["vx"] * speed * 2
        f["y"] += f["vy"] * speed * 2
        if f["x"] < 0 or f["x"] > buf_w:
            f["vx"] *= -1
        if f["y"] < 0 or f["y"] > buf_h:
            f["vy"] *= -1
        dist_sq = (xx - f["x"]) ** 2 + (yy - f["y"]) ** 2
        glow_mask = dist_sq < glow_size ** 2
        core_mask = dist_sq < base_size ** 2
        buf[glow_mask] = (buf[glow_mask] * (1 - alpha * 0.4) + f["color"] * alpha * 0.4).astype(np.uint8)
        buf[core_mask] = f["color"]
        alive.append(f)
    self.fireflies = alive
    h_scr, w_scr = self.height, self.width
    x_map = np.linspace(0, (buf_w - 1), w_scr, dtype=(np.int32))
    y_map = np.linspace(0, (buf_h - 1), h_scr, dtype=(np.int32))
    self.color_buffer[:] = buf[(y_map[:, None], x_map)]
    self.color_buffer_flag = 1
# ==== .LightEffectManager._update_hellfire_effect (linha 7827)
effect = self.effects["hellfire"]
speed = np.clip(effect["speed"], 1, 10) * 0.18
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
fire_main = colors[1]
fire_bright = colors[2]
spark_color = colors[3]
buf_w = 100
buf_h = 100
MARGIN = 8
max_particles = 24
max_sparks = 8
base_radius = 6
spark_size = 2
fade_strength = 0.15
if not hasattr(self, "hellfire_buf"):
    self.hellfire_buf = np.full((buf_h, buf_w, 3), bg_color, dtype=(np.uint8))
    self.hellfire_particles = []
    self.hellfire_sparks = []
    self.hellfire_cycle_hue = 0
buf = self.hellfire_buf
buf = (buf * (1 - fade_strength) + bg_color * fade_strength).astype(np.uint8)
yy, xx = np.indices((buf_h, buf_w), dtype=(np.float32))
self.hellfire_cycle_hue = (self.hellfire_cycle_hue + speed * 2) % 360
while len(self.hellfire_particles) < max_particles:
    self.hellfire_particles.append({'x':(np.random.uniform)(MARGIN, buf_w - MARGIN), 
     'y':buf_h - 10, 
     'rad':(np.random.uniform)(base_radius, base_radius * 2.2), 
     'life':(np.random.uniform)(60, 120), 
     'mod':(np.random.uniform)(0.5, 1.5)})

while len(self.hellfire_sparks) < max_sparks:
    self.hellfire_sparks.append({'x':(np.random.uniform)(MARGIN, buf_w - MARGIN), 
     'y':buf_h - 5, 
     'life':(np.random.uniform)(40, 80), 
     'mod':(np.random.uniform)(0.7, 1.3)})

alive_p = []
for p in self.hellfire_particles:
    p["life"] -= speed
    if p["life"] <= 0:
        pass
    else:
        p["x"] += np.sin(p["life"] * p["mod"]) * speed * 1.2
        p["y"] -= speed * 2.8
        p["rad"] = max(0.2, p["rad"] - speed * 0.25)
        p["x"] = np.clip(p["x"], MARGIN, buf_w - MARGIN)
        p["y"] = np.clip(p["y"], MARGIN, buf_h - MARGIN)
        dist_sq = (xx - p["x"]) ** 2 + (yy - p["y"]) ** 2
        mask = dist_sq < p["rad"] ** 2
        buf[mask] = fire_bright if p["rad"] > 4 else fire_main
        alive_p.append(p)
    self.hellfire_particles = alive_p
    alive_s = []

for s in self.hellfire_sparks:
    s["life"] -= speed
    if s["life"] <= 0:
        pass
    else:
        s["x"] += np.sin(s["life"] * s["mod"]) * speed * 1.5
        s["y"] -= speed * 4
        s["x"] = np.clip(s["x"], MARGIN, buf_w - MARGIN)
        s["y"] = np.clip(s["y"], MARGIN, buf_h - MARGIN)
        dist_sq = (xx - s["x"]) ** 2 + (yy - s["y"]) ** 2
        mask = dist_sq < spark_size ** 2
        buf[mask] = spark_color
        alive_s.append(s)
    self.hellfire_sparks = alive_s
    frame = np.clip(buf, 0, 255).astype(np.uint8)
    tw, th = self.width, self.height
    xs = np.linspace(0, (buf_w - 1), tw, dtype=int)
    ys = np.linspace(0, (buf_h - 1), th, dtype=int)
    self.color_buffer[:] = frame[np.ix_(ys, xs)]
    self.color_buffer_flag = 1
# ==== .LightEffectManager._update_dragon_flame_effect (linha 7956)
effect = self.effects["dragon_flame"]
speed = np.clip(effect["speed"], 1, 10) * 0.22
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg = colors[0]
fire_dark = colors[1]
fire_med = colors[2]
fire_bright = colors[3]
buf_w = 100
buf_h = 100
cx = buf_w // 2
particle_max = 20
base_radius = 5
shrink_rate = 0.18
fade = 0.25
rise_speed = 2.2
if not hasattr(self, "dragon_buf"):
    self.dragon_buf = np.full((buf_h, buf_w, 3), bg, dtype=(np.uint8))
    self.particles = []
buf = self.dragon_buf
buf = (buf * (1 - fade) + bg * fade).astype(np.uint8)
yy, xx = np.indices((buf_h, buf_w), dtype=(np.float32))
while len(self.particles) < particle_max:
    self.particles.append({'x':cx + (np.random.uniform(-8, 8)), 
     'y':buf_h - 5, 
     'rad':base_radius + (np.random.uniform(0, 3)), 
     'life':(np.random.uniform)(50, 90), 
     'hue':(np.random.uniform)(0, 1)})

alive = []
for p in self.particles:
    p["life"] -= speed
    if p["life"] <= 0:
        pass
    else:
        p["x"] += np.sin(p["life"] * 0.1) * speed * 0.8
        p["y"] -= rise_speed * speed
        p["rad"] = max(0.5, p["rad"] - shrink_rate * speed)
        if p["rad"] > 4:
            col = fire_bright
        elif p["rad"] > 2:
            col = fire_med
        else:
            col = fire_dark
        dist_sq = (xx - p["x"]) ** 2 + (yy - p["y"]) ** 2
        mask = dist_sq < p["rad"] ** 2
        buf[mask] = col
        alive.append(p)
    self.particles = alive
    h_scr, w_scr = self.height, self.width
    x_map = np.linspace(0, buf_w - 1, w_scr).astype(np.int32)
    y_map = np.linspace(0, buf_h - 1, h_scr).astype(np.int32)
    self.color_buffer[:] = buf[(y_map[:, None], x_map)]
    self.color_buffer_flag = 1
# ==== .LightEffectManager._update_hydrogen_effect (linha 8049)
import numpy as np
effect = self.effects["hydrogen"]
speed = np.clip(effect["speed"], 1, 10)
colors = effect["colors"]
RENDER_SIZE = 100
N_QUANTUM = 4
L_QUANTUM = 2
M_QUANTUM = 1
RADIUS_SCALE = 0.6
INTENSITY_BOOST = 120
ROTATE_SPEED = speed * 0.01
FADE_SPEED = speed * 0.005
if not hasattr(self, "_hy_angle"):
    self._hy_angle = 0.0
    self._hy_interp = 0.0
    self._hy_prev_density = np.zeros((RENDER_SIZE, RENDER_SIZE), dtype=(np.float32))
self._hy_angle = (self._hy_angle + ROTATE_SPEED) % (2 * np.pi)
self._hy_interp += FADE_SPEED
if self._hy_interp > 1.0:
    self._hy_interp = 0.0
y, x = np.indices((RENDER_SIZE, RENDER_SIZE))
cx, cy = RENDER_SIZE // 2, RENDER_SIZE // 2
dx = x - cx
dy = y - cy
cos_a = np.cos(self._hy_angle)
sin_a = np.sin(self._hy_angle)
x_rot = dx * cos_a - dy * sin_a
y_rot = dx * sin_a + dy * cos_a
r = np.sqrt(x_rot ** 2 + y_rot ** 2)
r_safe = np.where(r == 0, 1e-06, r)
cos_theta = x_rot / r_safe
rho = r * RADIUS_SCALE
radial = (2 / N_QUANTUM) ** 3 * np.exp(-rho) * rho ** (2 * L_QUANTUM)
laguerre = 1.0 - 2 * rho + 0.5 * rho ** 2
legendre = 1.5 * (3 * cos_theta ** 2 - 1) ** 2
density = radial * laguerre ** 2 * legendre * INTENSITY_BOOST
density = np.clip(density, 0.0, 1.0)
final_density = (1 - self._hy_interp) * self._hy_prev_density + self._hy_interp * density
self._hy_prev_density = density
color_num = len(colors)
color_arr = np.array(colors, dtype=(np.float32))
output = np.zeros((RENDER_SIZE, RENDER_SIZE, 3), dtype=(np.uint8))
if color_num >= 4:
    c0, c1, c2, c3 = color_arr
    mask1 = final_density < 0.33
    mask2 = (final_density >= 0.33) & (final_density < 0.66)
    mask3 = final_density >= 0.66
    output[mask1] = (c0 + (c1 - c0) * (final_density[(mask1, None)] / 0.33)).astype(np.uint8)
    output[mask2] = (c1 + (c2 - c1) * ((final_density[(mask2, None)] - 0.33) / 0.33)).astype(np.uint8)
    output[mask3] = (c2 + (c3 - c2) * ((final_density[(mask3, None)] - 0.66) / 0.34)).astype(np.uint8)
elif color_num == 3:
    c0, c1, c2 = color_arr
    mask_lo = final_density < 0.5
    mask_hi = ~mask_lo
    output[mask_lo] = (c0 + (c1 - c0) * (2 * final_density[(mask_lo, None)])).astype(np.uint8)
    output[mask_hi] = (c1 + (c2 - c1) * (2 * final_density[(mask_hi, None)] - 1)).astype(np.uint8)
else:
    c0, c1 = color_arr[0], color_arr[1]
    output = (c0 + (c1 - c0) * final_density[(Ellipsis, None)]).astype(np.uint8)
h, w = self.height, self.width
x_idx = np.linspace(0, (RENDER_SIZE - 1), w, dtype=int)
y_idx = np.linspace(0, (RENDER_SIZE - 1), h, dtype=int)
xv, yv = np.meshgrid(x_idx, y_idx)
self.color_buffer[:] = output[(yv, xv)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_hyperspace_effect (linha 8140)
import numpy as np
effect = self.effects["hyperspace"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.float32))
RENDER_SIZE = 100
N_STARS = 50
MOVE_DIR = 1
BASE_SIZE = 1.2
RING_COUNT = 40
bg_color = colors[0]
star_color = colors[1]
ring_color = colors[2]
if not hasattr(self, "_hs_x"):
    self._hs_x = np.random.uniform(0, RENDER_SIZE, N_STARS).astype(np.float32)
    self._hs_y = np.random.uniform(0, RENDER_SIZE, N_STARS).astype(np.float32)
    self._hs_z = np.random.uniform(0.1, RENDER_SIZE, N_STARS).astype(np.float32)
    self._hs_ring_t = 0.0
cx = cy = RENDER_SIZE // 2
move_step = speed * 0.9 * MOVE_DIR
self._hs_z -= move_step
reset_mask = self._hs_z <= 0
self._hs_z[reset_mask] = RENDER_SIZE
self._hs_x[reset_mask] = np.random.uniform(0, RENDER_SIZE, np.sum(reset_mask))
self._hs_y[reset_mask] = np.random.uniform(0, RENDER_SIZE, np.sum(reset_mask))
z_safe = np.maximum(self._hs_z, 1.0)
scale = RENDER_SIZE / z_safe
x2d = (self._hs_x - cx) * scale + cx
y2d = (self._hs_y - cy) * scale + cy
size = scale * BASE_SIZE
alpha = np.clip(2.2 - scale / 4, 0.2, 1.0)
yidx, xidx = np.indices((RENDER_SIZE, RENDER_SIZE), dtype=(np.float32))
frame = np.full((RENDER_SIZE, RENDER_SIZE, 3), bg_color, dtype=(np.float32))
dx = xidx[(Ellipsis, None)] - x2d
dy = yidx[(Ellipsis, None)] - y2d
dist = np.sqrt(dx * dx + dy * dy)
star_mask = dist < size
alpha_mask = star_mask * alpha
sum_alpha = np.sum(alpha_mask, axis=(-1), keepdims=True)
frame = frame * (1 - sum_alpha) + star_color * sum_alpha
self._hs_ring_t += speed * 0.08
t = self._hs_ring_t
r = np.linspace(2, (RENDER_SIZE // 2), RING_COUNT, dtype=(np.float32))
dr = RENDER_SIZE / RING_COUNT * 0.5
dist_center = np.sqrt((xidx - cx) ** 2 + (yidx - cy) ** 2)
for ri in r:
    ring_mask = np.abs(dist_center - ri) < dr
    frame[ring_mask] = frame[ring_mask] * 0.75 + ring_color * 0.25

frame = np.clip(frame, 0, 255).astype(np.uint8)
h, w = self.height, self.width
xs = np.linspace(0, (RENDER_SIZE - 1), w, dtype=int)
ys = np.linspace(0, (RENDER_SIZE - 1), h, dtype=int)
xv, yv = np.meshgrid(xs, ys)
self.color_buffer[:] = frame[(yv, xv)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_jellyfish_effect (linha 8221)
import numpy as np
effect = self.effects["jellyfish"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.float32))
RENDER_SIZE = 100
ARMS = 9
POINTS = 5
TRACE = 0.95
MOVE_SPEED = speed * 0.08
bg = colors[0]
c1 = colors[1] if len(colors) >= 2 else np.array([30, 180, 220], np.float32)
cx, cy = RENDER_SIZE / 2, RENDER_SIZE / 2
if not hasattr(self, "_jf_buf"):
    self._jf_buf = np.full((RENDER_SIZE, RENDER_SIZE, 3), bg, np.float32)
    self._jf_t = 0.0
    self._jf_x = np.random.uniform(0, RENDER_SIZE, (ARMS, POINTS))
    self._jf_y = np.random.uniform(0, RENDER_SIZE, (ARMS, POINTS))
self._jf_t += MOVE_SPEED
t = self._jf_t
tx = cx + 28 * np.cos(t * 0.5)
ty = cy + 28 * np.sin(t * 0.4)
r = 18 + 6 * np.sin(t * 0.8)
angles = np.linspace(0, 2 * np.pi, ARMS)
hx = tx + r * np.cos(angles)
hy = ty + r * np.sin(angles)
px = self._jf_x
py = self._jf_y
for i in range(POINTS):
    lx = hx if i == 0 else px[:, i - 1]
    ly = hy if i == 0 else py[:, i - 1]
    dx = px[:, i] - lx
    dy = py[:, i] - ly
    dist = np.sqrt(dx ** 2 + dy ** 2 + 1e-08)
    px[:, i] = lx + dx * 2.5 / dist
    py[:, i] = ly + dy * 2.5 / dist

self._jf_buf *= TRACE
self._jf_buf += bg * (1 - TRACE)
yv, xv = np.indices((RENDER_SIZE, RENDER_SIZE))
glow = np.zeros((RENDER_SIZE, RENDER_SIZE), np.float32)
for arm in range(ARMS):
    for i in range(POINTS):
        if i == 0:
            x0, y0 = hx[arm], hy[arm]
        else:
            x0, y0 = px[(arm, i - 1)], py[(arm, i - 1)]
        x1, y1 = px[(arm, i)], py[(arm, i)]
        dx_seg = x1 - x0
        dy_seg = y1 - y0
        len2 = dx_seg ** 2 + dy_seg ** 2 + 1e-08
        t = ((xv - x0) * dx_seg + (yv - y0) * dy_seg) / len2
        t = np.clip(t, 0, 1)
        px_n = x0 + t * dx_seg
        py_n = y0 + t * dy_seg
        d = np.sqrt((xv - px_n) ** 2 + (yv - py_n) ** 2)
        glow += np.exp(-d * 0.75) * 0.18

self._jf_buf += glow[(Ellipsis, None)] * c1
frame = np.clip(self._jf_buf, 0, 255).astype(np.uint8)
h, w = self.height, self.width
xs = np.linspace(0, 99, w, dtype=int)
ys = np.linspace(0, 99, h, dtype=int)
self.color_buffer[:] = frame[np.ix_(ys, xs)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_koi_pond_effect (linha 8314)
import numpy as np
effect = self.effects["koi_pond"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.float32))
RENDER_SIZE = 100
FISH_COUNT = 5
FISH_TRAIL = 10
LILY_COUNT = 5
WATER_COUNT = 8
FADE = 0.9
MOVE_SPEED = speed * 0.12
bg = colors[0]
fish_c1 = colors[1]
fish_c2 = colors[2]
lily_c = colors[3]
cx, cy = RENDER_SIZE // 2, RENDER_SIZE // 2
if not hasattr(self, "_koi_buf"):
    self._koi_buf = np.full((RENDER_SIZE, RENDER_SIZE, 3), bg, dtype=(np.float32))
    self._t = 0.0
    self._fish_pos = np.random.uniform(20, 80, (FISH_COUNT, FISH_TRAIL, 2))
    self._fish_vel = np.random.uniform(-0.5, 0.5, (FISH_COUNT, 2))
    self._lily_x = np.random.uniform(20, 80, LILY_COUNT)
    self._lily_y = np.random.uniform(20, 80, LILY_COUNT)
    self._lily_vx = np.random.uniform(-0.3, 0.3, LILY_COUNT)
    self._lily_vy = np.random.uniform(-0.3, 0.3, LILY_COUNT)
    self._water_x = np.random.uniform(0, RENDER_SIZE, WATER_COUNT)
    self._water_y = np.random.uniform(0, RENDER_SIZE, WATER_COUNT)
self._t += MOVE_SPEED
buf = self._koi_buf
buf *= FADE
buf += bg * (1 - FADE)
yy, xx = np.indices((RENDER_SIZE, RENDER_SIZE), dtype=(np.float32))
for i in range(FISH_COUNT):
    self._fish_vel[i] += (np.array([cx, cy]) - self._fish_pos[(i, 0)]) * 0.0005
    self._fish_pos[i, 1:] = self._fish_pos[i, :-1]
    self._fish_pos[(i, 0)] += self._fish_vel[i] * MOVE_SPEED
    pos = self._fish_pos[(i, 0)]
    vel = self._fish_vel[i]
    vel[pos < 5] *= -1
    vel[pos > RENDER_SIZE - 5] *= -1
    for j in range(FISH_TRAIL):
        x, y = self._fish_pos[(i, j)]
        r = 6 - j * 0.5
        d = np.sqrt((xx - x) ** 2 + (yy - y) ** 2)
        mask = d < r
        a = np.exp(-d / 3) * 0.3
        buf[mask] += a[(mask, None)] * (fish_c1 if j % 2 == 0 else fish_c2)

self._lily_x += self._lily_vx
self._lily_y += self._lily_vy
mask_x = (self._lily_x < 5) | (self._lily_x > RENDER_SIZE - 5)
mask_y = (self._lily_y < 5) | (self._lily_y > RENDER_SIZE - 5)
self._lily_vx[mask_x] *= -1
self._lily_vy[mask_y] *= -1
for i in range(LILY_COUNT):
    d = np.sqrt((xx - self._lily_x[i]) ** 2 + (yy - self._lily_y[i]) ** 2)
    buf[d < 6] += 0.4 * lily_c

for i in range(WATER_COUNT):
    x = (self._water_x[i] + self._t * 3) % RENDER_SIZE
    y = (self._water_y[i] + self._t * 2) % RENDER_SIZE
    d = np.abs(np.sqrt((xx - x) ** 2 + (yy - y) ** 2) - 12) < 1.5
    buf[d] += [4, 8, 12]

frame = np.clip(buf, 0, 255).astype(np.uint8)
h, w = self.height, self.width
xs = np.linspace(0, (RENDER_SIZE - 1), w, dtype=int)
ys = np.linspace(0, (RENDER_SIZE - 1), h, dtype=int)
self.color_buffer[:] = frame[np.ix_(ys, xs)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_lasers_effect (linha 8418)
import numpy as np
effect = self.effects["lasers"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.float32))
RENDER_SIZE = 100
BEAM_COUNT = 1
BEAM_WIDTH = 4
FADE = 0.9
MOVE_SPEED = speed * 0.01
bg = colors[0]
c1 = colors[1] if len(colors) >= 2 else [255, 8, 1]
c2 = colors[2] if len(colors) >= 3 else [0, 84, 249]
c3 = colors[3] if len(colors) >= 4 else [35, 255, 49]
h, w = RENDER_SIZE, RENDER_SIZE
if not hasattr(self, "_laser_buf"):
    self._laser_buf = np.full((h, w, 3), bg, dtype=(np.float32))
    self._laser_t = 0.0
    self._yy, self._xx = np.indices((h, w), dtype=(np.float32))
buf = self._laser_buf
xx, yy = self._xx, self._yy
self._laser_t += MOVE_SPEED
rot = self._laser_t * 0.8
buf *= FADE
buf += bg * (1 - FADE)
angles = np.linspace(0, np.pi * 0.6, BEAM_COUNT) + rot

def draw(x0, y0, color):
    for a in angles:
        dx = np.cos(a)
        dy = np.sin(a)
        t = (dx * (xx - x0) + dy * (yy - y0)) / (dx ** 2 + dy ** 2 + 1e-08)
        t = np.clip(t, 0, 100)
        px = x0 + t * dx
        py = y0 + t * dy
        d = np.sqrt((xx - px) ** 2 + (yy - py) ** 2)
        mask = d < BEAM_WIDTH
        buf[mask] += np.exp(-d[mask] * 0.3)[:, None] * color * 0.4


draw(0, h, c1)
draw(w, h, c2)
draw(0, 0, c3)
draw(w, 0, c1)
target_h, target_w = self.height, self.width
xs = np.linspace(0, (w - 1), target_w, dtype=int)
ys = np.linspace(0, (h - 1), target_h, dtype=int)
self.color_buffer[:] = buf[np.ix_(ys, xs)].clip(0, 255).astype(np.uint8)
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_lasers_effect.draw (linha 8459)
for a in angles:
    dx = np.cos(a)
    dy = np.sin(a)
    t = (dx * (xx - x0) + dy * (yy - y0)) / (dx ** 2 + dy ** 2 + 1e-08)
    t = np.clip(t, 0, 100)
    px = x0 + t * dx
    py = y0 + t * dy
    d = np.sqrt((xx - px) ** 2 + (yy - py) ** 2)
    mask = d < BEAM_WIDTH
    buf[mask] += np.exp(-d[mask] * 0.3)[:, None] * color * 0.4
# ==== .LightEffectManager._update_lava_lamp_effect (linha 8484)
import numpy as np
effect = self.effects["lava_lamp"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.float32))
RENDER_SIZE = 100
BLOB_COUNT = 7
BLOB_MIN_SIZE = 8
BLOB_MAX_SIZE = 18
FADE = 0.92
MOVE_SPEED = speed * 0.2
SMOOTH = 0.15
bg = colors[0]
c1 = colors[1] if len(colors) >= 2 else [0, 13, 237]
c2 = colors[2] if len(colors) >= 3 else [228, 0, 32]
h, w = RENDER_SIZE, RENDER_SIZE
if not hasattr(self, "_lava_buf"):
    self._lava_buf = np.full((h, w, 3), bg, dtype=(np.float32))
    self._lava_t = 0.0
    self._lava_blobs = np.zeros((BLOB_COUNT, 5), dtype=(np.float32))
    self._lava_blobs[:, 0] = np.random.uniform(20, w - 20, BLOB_COUNT)
    self._lava_blobs[:, 1] = np.random.uniform(20, h - 20, BLOB_COUNT)
    self._lava_blobs[:, 2] = np.random.uniform(-0.8, 0.8, BLOB_COUNT)
    self._lava_blobs[:, 3] = np.random.uniform(-0.8, 0.8, BLOB_COUNT)
    self._lava_blobs[:, 4] = np.random.uniform(BLOB_MIN_SIZE, BLOB_MAX_SIZE, BLOB_COUNT)
    self._yy, self._xx = np.indices((h, w), dtype=(np.float32))
buf = self._lava_buf
xx, yy = self._xx, self._yy
blobs = self._lava_blobs
self._lava_t += MOVE_SPEED
blobs[:, 0] += blobs[:, 2] * MOVE_SPEED
blobs[:, 1] += blobs[:, 3] * MOVE_SPEED
mask_xl = blobs[:, 0] < blobs[:, 4]
mask_xr = blobs[:, 0] > w - blobs[:, 4]
mask_yt = blobs[:, 1] < blobs[:, 4]
mask_yb = blobs[:, 1] > h - blobs[:, 4]
blobs[(mask_xl | mask_xr, 2)] *= -1
blobs[(mask_yt | mask_yb, 3)] *= -1
buf *= FADE
buf += bg * (1 - FADE)
field = np.zeros((h, w), dtype=(np.float32))
for i in range(BLOB_COUNT):
    x = blobs[(i, 0)]
    y = blobs[(i, 1)]
    r = blobs[(i, 4)]
    dist = np.sqrt((xx - x) ** 2 + (yy - y) ** 2)
    field += r ** 2 / (dist ** 2 + 1e-06)

glow1 = np.clip(field - 1.0, 0, 1)
glow2 = np.clip(field - 1.1, 0, 1)
buf += glow1[(Ellipsis, None)] * c1 * SMOOTH
buf += glow2[(Ellipsis, None)] * c2 * SMOOTH * 0.7
frame = np.clip(buf, 0, 255).astype(np.uint8)
target_h, target_w = self.height, self.width
xs = np.linspace(0, (w - 1), target_w, dtype=int)
ys = np.linspace(0, (h - 1), target_h, dtype=int)
self.color_buffer[:] = frame[np.ix_(ys, xs)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_mario_effect (linha 8575)
import numpy as np
effect = self.effects["mario"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.float32))
RENDER_SIZE = 100
GROUND = 85
MARIO_W = 10
MARIO_H = 15
BLOCK_W = 24
BLOCK_H = 15
COIN_SIZE = 7
COIN_FADE_SPEED = 0.8
BLOCK_SCROLL_SPEED = speed * 0.09
JUMP_GRAVITY = 0.12
JUMP_FORCE = -3.2
bg = colors[0]
c_mario = colors[1] if len(colors) >= 2 else [240, 80, 0]
c_coin = colors[2] if len(colors) >= 3 else [255, 220, 0]
c_block = colors[3] if len(colors) >= 4 else [120, 80, 40]
c_ground = c_block * 0.4
h, w = RENDER_SIZE, RENDER_SIZE
if not hasattr(self, "_mario_buf"):
    self._mario_buf = np.full((h, w, 3), bg, dtype=(np.float32))
    self._yy, self._xx = np.indices((h, w), dtype=(np.float32))
    self.mario_x = 30
    self.mario_y = GROUND - MARIO_H
    self.mario_vy = 0.0
    self.is_jumping = False
    self.blocks = np.empty((0, 3), dtype=(np.float32))
    self.coins = np.empty((0, 3), dtype=(np.float32))
    self.block_spawn_timer = 0
buf = self._mario_buf
xx, yy = self._xx, self._yy
buf[:] = bg
buf[yy >= GROUND] = c_ground
self.block_spawn_timer += 1
if self.block_spawn_timer > max(40, 90 - int(speed * 3)):
    self.block_spawn_timer = 0
    new_x = w + np.random.randint(0, 30)
    new_y = 42
    self.blocks = np.vstack([self.blocks, [new_x, new_y, 1.0]])
self.blocks[:, 0] -= BLOCK_SCROLL_SPEED
self.blocks = self.blocks[self.blocks[:, 0] > -BLOCK_W]
if not self.is_jumping:
    self.mario_vy = JUMP_FORCE
    self.is_jumping = True
    self.mario_x = 25 + np.random.uniform(-3, 8)
self.mario_vy += JUMP_GRAVITY
self.mario_y += self.mario_vy
if self.mario_y >= GROUND - MARIO_H:
    self.mario_y = GROUND - MARIO_H
    self.mario_vy = 0
    self.is_jumping = False
mx = int(round(self.mario_x))
my = int(round(self.mario_y))
for i in range(len(self.blocks)):
    bx, by, active = self.blocks[i]
    bxi = int(round(bx))
    byi = int(round(by))
    hit = self.is_jumping and self.mario_vy < 0 and abs(mx - bxi) < 10 and abs(my - byi) < 12
    if hit:
        if active > 0.5:
            self.blocks[(i, 2)] = 0
            self.coins = np.vstack([self.coins, [bx, by - 8, 50.0]])
        mask = (xx >= bxi - BLOCK_W // 2) & (xx <= bxi + BLOCK_W // 2) & (yy >= byi - BLOCK_H // 2) & (yy <= byi + BLOCK_H // 2)
        buf[mask] = c_block

if len(self.coins) > 0:
    self.coins[:, 1] -= 0.6
    self.coins[:, 2] -= COIN_FADE_SPEED
    for cx, cy, life in self.coins:
        if life > 0:
            alpha = life / 50.0
            dist = np.sqrt((xx - cx) ** 2 + (yy - cy) ** 2)
            buf[dist < COIN_SIZE] = c_coin * alpha + buf[dist < COIN_SIZE] * (1 - alpha)
        self.coins = self.coins[self.coins[:, 2] > 0]

buf[(yy >= my) & (yy <= my + 12) & (xx >= mx) & (xx <= mx + 10)] = c_mario
buf[(yy == my - 2) & (xx >= mx + 3) & (xx <= mx + 7)] = c_mario
buf[(yy == my + 13) & (xx == mx + 2)] = c_mario
buf[(yy == my + 13) & (xx == mx + 8)] = c_mario
frame = np.clip(buf, 0, 255).astype(np.uint8)
th, tw = self.height, self.width
xs = np.linspace(0, 99, tw, dtype=int)
ys = np.linspace(0, 99, th, dtype=int)
self.color_buffer[:] = frame[np.ix_(ys, xs)]
self.color_buffer_flag = 1
# ==== .LightEffectManager._update_multiverse_simple_effect (linha 8702)
import numpy as np
effect = self.effects["multiverse_simple"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.float32))
line_color = colors[0]
main_color = colors[1]
W, H = (100, 100)
ZOOM = 6.0
LINE_THRESH = 0.2
LINE_OFFSET = -0.1
ANIM_SPEED = speed * 0.002
if not hasattr(self, "_mv_buf"):
    self._mv_buf = np.zeros((H, W, 3), dtype=(np.float32))
    self._yy, self._xx = np.indices((H, W), dtype=(np.float32))
    self._mv_time = 0.0
buf = self._mv_buf
xx, yy = self._xx, self._yy
self._mv_time += ANIM_SPEED
t = self._mv_time
n = np.sin(t) * ZOOM
m = np.cos(t) * ZOOM
pi = np.pi
nx = n * pi * xx / 300
my = m * pi * yy / 300
mx = m * pi * xx / 300
ny = n * pi * yy / 300
d = np.cos(nx) * np.cos(my) - np.cos(mx) * np.cos(ny)
buf[:] = line_color
mask_main = np.abs(d - LINE_OFFSET) >= LINE_THRESH
buf[mask_main] = main_color
mask_line = np.abs(d - LINE_OFFSET) < LINE_THRESH
buf[mask_line] = line_color
frame = np.clip(buf, 0, 255).astype(np.uint8)
tw, th = self.width, self.height
xs = np.linspace(0, (W - 1), tw, dtype=int)
ys = np.linspace(0, (H - 1), th, dtype=int)
self.color_buffer[:] = frame[np.ix_(ys, xs)]
self.color_buffer_flag = 1
# ==== .LightEffectManager.render_cosmic_nebula (linha 8765)
effect = self.effects["cosmic_nebula"]
speed = effect["speed"]
colors = effect["colors"]
LOW_RES = 100
CENTER_POS = 50
NEBULA_LAYERS = 3
ANIM_SPEED = 0.02
WAVE_FREQ = 4
FADE_COEFF = 2.5
BASE_ALPHA = 0.5
STAR_CHANCE = 0.03
if not hasattr(self, "cosmic_time"):
    self.cosmic_time = 0
y, x = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
norm_x = (x - CENTER_POS) / CENTER_POS
norm_y = (y - CENTER_POS) / CENTER_POS
buffer = np.full((LOW_RES, LOW_RES, 3), (colors[0]), dtype=(np.float32))
time = self.cosmic_time * (ANIM_SPEED + speed * 0.004)
self.cosmic_time += 1
for i in range(NEBULA_LAYERS):
    angle = time + i * 0.6
    cos_a, sin_a = np.cos(angle), np.sin(angle)
    rot_x = norm_x * cos_a - norm_y * sin_a
    rot_y = norm_x * sin_a + norm_y * cos_a
    dist = np.sqrt(rot_x ** 2 + rot_y ** 2)
    wave = np.sin(dist * WAVE_FREQ - time * 1.2) * 0.5 + 0.5
    mask = np.exp(-dist ** FADE_COEFF) * wave * BASE_ALPHA
    c_idx = 1 + i % (len(colors) - 1) if len(colors) > 1 else 0
    r, g, b = colors[c_idx]
    buffer[(Ellipsis, 0)] += r * mask
    buffer[(Ellipsis, 1)] += g * mask
    buffer[(Ellipsis, 2)] += b * mask

if len(colors) >= 4:
    star_mask = np.random.rand(LOW_RES, LOW_RES) < STAR_CHANCE
    buffer[star_mask] = colors[3]
frame = np.clip(buffer, 0, 255).astype(np.uint8)
tw, th = self.width, self.height
xs = np.linspace(0, (LOW_RES - 1), tw, dtype=int)
ys = np.linspace(0, (LOW_RES - 1), th, dtype=int)
self.color_buffer[:] = frame[np.ix_(ys, xs)]
self.color_buffer_flag = 1
# ==== .LightEffectManager.render_neon_grid (linha 8834)
effect = self.effects["neon_grid"]
speed = effect["speed"]
colors = effect["colors"]
LOW_RES = 100
CENTER = 50
LINE_SPEED = speed * 0.01
LINE_WIDTH = 0.1
GRID_SPACING = 10
ANIM_FADE = speed * 0.1
COLOR_CYCLE_SPEED = speed * 2
if not hasattr(self, "neon_time"):
    self.neon_time = 0
y, x = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
buffer = np.full((LOW_RES, LOW_RES, 3), (colors[0]), dtype=(np.float32))
time = self.neon_time * (LINE_SPEED + speed * 0.005)
self.neon_time += 1
x_lines = np.arange(0, LOW_RES, GRID_SPACING)
for px in x_lines:
    mask = np.abs(x - px - np.sin(time) * 5) < LINE_WIDTH
    buffer[mask] = colors[1]

y_lines = np.arange(0, LOW_RES, GRID_SPACING)
for py in y_lines:
    mask = np.abs(y - py + np.cos(time) * 5) < LINE_WIDTH
    buffer[mask] = colors[1]

move_offset = int(time * speed * 2 % LOW_RES)
h_mask = np.abs(y - move_offset) < LINE_WIDTH + 1
buffer[h_mask] = colors[1] if len(colors) < 3 else colors[2]
v_mask = np.abs(x - (LOW_RES - move_offset)) < LINE_WIDTH + 1
buffer[v_mask] = colors[1] if len(colors) < 4 else colors[3]
buffer *= ANIM_FADE
frame = np.clip(buffer, 0, 255).astype(np.uint8)
tw, th = self.width, self.height
xs = np.linspace(0, (LOW_RES - 1), tw, dtype=int)
ys = np.linspace(0, (LOW_RES - 1), th, dtype=int)
self.color_buffer[:] = frame[np.ix_(ys, xs)]
self.color_buffer_flag = 1
# ==== .LightEffectManager.render_neurolink (linha 8896)
effect = self.effects["neurolink"]
speed = effect["speed"]
colors = effect["colors"]
LOW_RES = 100
MARGIN = 6
PARTICLE_NUM = 21
PARTICLE_SIZE = 3
LINK_DIST = 30
BASE_VEL = 0.5
FADE = 0.92
if not hasattr(self, "part_pos"):
    self.part_pos = np.random.uniform(MARGIN, LOW_RES - MARGIN, (PARTICLE_NUM, 2))
    self.part_vel = np.random.uniform(-1, 1, (PARTICLE_NUM, 2)) * BASE_VEL
if not hasattr(self, "neuro_t"):
    self.neuro_t = 0
y_g, x_g = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
buf = np.full((LOW_RES, LOW_RES, 3), (colors[0]), dtype=(np.float32))
buf *= FADE
self.part_pos += self.part_vel * speed
mask_x = (self.part_pos[:, 0] < MARGIN) | (self.part_pos[:, 0] > LOW_RES - MARGIN)
self.part_vel[(mask_x, 0)] *= -1
mask_y = (self.part_pos[:, 1] < MARGIN) | (self.part_pos[:, 1] > LOW_RES - MARGIN)
self.part_vel[(mask_y, 1)] *= -1
self.part_pos[:, 0] = np.clip(self.part_pos[:, 0], MARGIN, LOW_RES - MARGIN)
self.part_pos[:, 1] = np.clip(self.part_pos[:, 1], MARGIN, LOW_RES - MARGIN)
p_color = colors[1] if len(colors) >= 4 else colors[1]
for i in range(PARTICLE_NUM):
    px, py = self.part_pos[i]
    dist_sq = (x_g - px) ** 2 + (y_g - py) ** 2
    mask = dist_sq < PARTICLE_SIZE ** 2
    buf[mask] = p_color

if len(colors) >= 3:
    line_c = colors[2]
    for i in range(PARTICLE_NUM):
        x1, y1 = self.part_pos[i]
        for j in range(i + 1, PARTICLE_NUM):
            x2, y2 = self.part_pos[j]
            d_sq = (x1 - x2) ** 2 + (y1 - y2) ** 2
            if d_sq < LINK_DIST ** 2:
                steps = int(np.sqrt(d_sq))
                xs = np.linspace(x1, x2, steps, dtype=int)
                ys = np.linspace(y1, y2, steps, dtype=int)
                valid = (xs >= 0) & (xs < LOW_RES) & (ys >= 0) & (ys < LOW_RES)
                buf[(ys[valid], xs[valid])] = line_c

self.neuro_t += 1
frame = np.clip(buf, 0, 255).astype(np.uint8)
tw, th = self.width, self.height
xs = np.linspace(0, (LOW_RES - 1), tw, dtype=int)
ys = np.linspace(0, (LOW_RES - 1), th, dtype=int)
self.color_buffer[:] = frame[np.ix_(ys, xs)]
self.color_buffer_flag = 1
# ==== .LightEffectManager.render_new_years (linha 8977)
effect = self.effects["new_years"]
speed = effect["speed"]
colors = effect["colors"]
LOW_RES = 100
MARGIN = 6
FIREWORK_COUNT = 8
FIREWORK_SIZE = 2
EXPLOSION_SIZE = 4
EXPLOSION_DIST = 20
EXPLOSION_PARTICLES = 12
FADE_ALPHA = 0.9
SPAWN_RATE = 0.03
if not hasattr(self, "fireworks"):
    self.fireworks = []
    self.explosions = []
if not hasattr(self, "new_year_t"):
    self.new_year_t = 0
y_g, x_g = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
buf = np.full((LOW_RES, LOW_RES, 3), (colors[0]), dtype=(np.float32))
buf *= FADE_ALPHA
if np.random.rand() < SPAWN_RATE * (speed / 5):
    if len(self.fireworks) < FIREWORK_COUNT:
        fx = np.random.uniform(MARGIN, LOW_RES - MARGIN)
        fy = LOW_RES - MARGIN
        self.fireworks.append([fx, fy, 0, -1.0 * (speed / 4)])
alive_fireworks = []
for fw in self.fireworks:
    x, y, lt, vy = fw
    y += vy
    lt += 1
    if y < MARGIN * 3 or lt > 40:
        for _ in range(EXPLOSION_PARTICLES):
            vx = (np.random.rand() - 0.5) * EXPLOSION_DIST * (speed / 5)
            vy = (np.random.rand() - 0.5) * EXPLOSION_DIST * (speed / 5)
            self.explosions.append([x,y,vx,vy,30])

    else:
        alive_fireworks.append([x, y, lt, vy])
        dist_sq = (x_g - x) ** 2 + (y_g - y) ** 2
        mask = dist_sq < FIREWORK_SIZE ** 2
        buf[mask] = colors[1]
    self.fireworks = alive_fireworks
    alive_explosions = []

exp_color = colors[2] if len(colors) >= 3 else colors[1]
highlight_color = colors[3] if len(colors) >= 4 else exp_color
for ep in self.explosions:
    x, y, vx, vy, lt = ep
    if lt <= 0:
        pass
    else:
        x += vx
        y += vy
        vy += 0.1
        lt -= 1
        x = np.clip(x, MARGIN, LOW_RES - MARGIN)
        y = np.clip(y, MARGIN, LOW_RES - MARGIN)
        alive_explosions.append([x,y,vx,vy,lt])
        dist_sq = (x_g - x) ** 2 + (y_g - y) ** 2
        mask = dist_sq < EXPLOSION_SIZE ** 2
        buf[mask] = highlight_color if lt > 15 else exp_color
    self.explosions = alive_explosions
    self.new_year_t += 1
    frame = np.clip(buf, 0, 255).astype(np.uint8)
    tw, th = self.width, self.height
    xs = np.linspace(0, (LOW_RES - 1), tw, dtype=int)
    ys = np.linspace(0, (LOW_RES - 1), th, dtype=int)
    self.color_buffer[:] = frame[np.ix_(ys, xs)]
    self.color_buffer_flag = 1
# ==== .LightEffectManager.render_paint_splatters (linha 9077)
effect = self.effects["paint_splatters"]
speed = np.clip(effect["speed"], 1, 10) * 0.12
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
main_color = colors[1]
bright_color = colors[1] if len(colors) >= 3 else main_color
rainbow_color = colors[2] if len(colors) >= 4 else bright_color
LOW_RES = 100
MAX_PARTICLES = 20
BASE_RADIUS = 10
NOISE_SCALE = 30
NOISE_POWER = 2.5
NOISE_SPEED = 0.015
FADE_STRENGTH = 0.18
RAINBOE_MODE = True
if not hasattr(self, "paint_buf"):
    self.paint_buf = np.full((LOW_RES, LOW_RES, 3), bg_color, dtype=(np.uint8))
    self.paint_particles = []
    self.paint_time = 0
buf = self.paint_buf
buf = (buf * (1 - FADE_STRENGTH) + bg_color * FADE_STRENGTH).astype(np.uint8)
yy, xx = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
self.paint_time += NOISE_SPEED

def simple_noise(x, y, t):
    return np.sin(x * 0.1 + t) * np.cos(y * 0.1 + t * 0.8) * 0.5


while len(self.paint_particles) < MAX_PARTICLES:
    self.paint_particles.append({'x':(np.random.uniform)(0, LOW_RES), 
     'y':(np.random.uniform)(0, LOW_RES), 
     'rad':BASE_RADIUS + (np.random.uniform(0, 1)), 
     'color_offset':(np.random.uniform)(0, 1)})

alive_particles = []
for p in self.paint_particles:
    vx = simple_noise(p["y"] / NOISE_SCALE, 0, self.paint_time) * NOISE_POWER
    vy = simple_noise(p["x"] / NOISE_SCALE, 0, self.paint_time) * NOISE_POWER
    p["x"] += vx * speed
    p["y"] += vy * speed
    if p["x"] > LOW_RES:
        p["x"] = 0
    elif p["x"] < 0:
        p["x"] = LOW_RES
    if p["y"] > LOW_RES:
        p["y"] = 0
    elif p["y"] < 0:
        p["y"] = LOW_RES
    if RAINBOE_MODE:
        current_col = rainbow_color if p["color_offset"] > 0.5 else bright_color
    else:
        current_col = bright_color if np.abs(vx + vy) > 1 else main_color
    dist_sq = (xx - p["x"]) ** 2 + (yy - p["y"]) ** 2
    mask = dist_sq < p["rad"] ** 2
    buf[mask] = current_col
    alive_particles.append(p)

self.paint_particles = alive_particles
frame = np.clip(buf, 0, 255).astype(np.uint8)
tw, th = self.width, self.height
xs = np.linspace(0, (LOW_RES - 1), tw, dtype=int)
ys = np.linspace(0, (LOW_RES - 1), th, dtype=int)
self.color_buffer[:] = frame[np.ix_(ys, xs)]
self.color_buffer_flag = 1
# ==== .LightEffectManager.render_paint_splatters.simple_noise (linha 9120)
return np.sin(x * 0.1 + t) * np.cos(y * 0.1 + t * 0.8) * 0.5
# ==== .LightEffectManager.render_party_parrot (linha 9187)
effect = self.effects["party_parrot"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
body_color = colors[1]
eye_color = colors[2]
beak_color = colors[3]
LOW_RES = 100
CENTER_X = LOW_RES // 2
BODY_HEIGHT = 20
BODY_TOP_Y = LOW_RES - BODY_HEIGHT
HEAD_SIZE = 18
BASE_NECK = 12
MOVE_SPEED = speed * 0.07
AMP_X = 30
AMP_Y = 32
RANDOM_SHAKE = speed * 0.012
if not hasattr(self, "parrot_buf"):
    self.parrot_buf = np.full((LOW_RES, LOW_RES, 3), bg_color, dtype=(np.uint8))
    self.anim_phase = 0
    self.rand_x = 0
    self.rand_y = 0
buf = self.parrot_buf
buf[:] = bg_color
Y, X = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
self.anim_phase += MOVE_SPEED
self.rand_x = np.clip(self.rand_x + np.random.uniform(-RANDOM_SHAKE, RANDOM_SHAKE), -10, 10)
self.rand_y = np.clip(self.rand_y + np.random.uniform(-RANDOM_SHAKE, RANDOM_SHAKE), -8, 8)
head_x = CENTER_X + np.sin(self.anim_phase * 0.5) * AMP_X + self.rand_x
head_y = BODY_TOP_Y - 20 - np.cos(self.anim_phase * 0.45) * AMP_Y + self.rand_y
neck_length = BODY_TOP_Y - head_y
neck_top_w = max(2, BASE_NECK - (AMP_Y - (BODY_TOP_Y - head_y)) * 0.35)
neck_bottom_w = BASE_NECK
body_mask = Y >= BODY_TOP_Y
buf[body_mask] = body_color
progress = (Y - head_y) / neck_length
progress = np.clip(progress, 0, 1)
current_neck_w = neck_top_w + (neck_bottom_w - neck_top_w) * progress
neck_mask = (np.abs(X - head_x) < current_neck_w / 2) & (Y >= head_y) & (Y <= BODY_TOP_Y)
curve_transition = ((X - head_x) / (BASE_NECK * 1.6)) ** 2 + ((Y - BODY_TOP_Y) / 10) ** 2 < 1.0
neck_mask |= curve_transition
buf[neck_mask] = body_color
head_mask = (X - head_x) ** 2 + (Y - head_y) ** 2 < HEAD_SIZE ** 2
buf[head_mask] = body_color
eye1 = (X - (head_x - 5)) ** 2 + (Y - (head_y - 4)) ** 2 < 9
eye2 = (X - (head_x + 5)) ** 2 + (Y - (head_y - 4)) ** 2 < 9
buf[eye1 | eye2] = eye_color
beak_mask = (X >= head_x - 3) & (X <= head_x + 3) & (Y >= head_y + 5) & (Y <= head_y + 9)
buf[beak_mask] = beak_color
frame = np.clip(buf, 0, 255).astype(np.uint8)
tw, th = self.width, self.height
xs = np.linspace(0, (LOW_RES - 1), tw, dtype=int)
ys = np.linspace(0, (LOW_RES - 1), th, dtype=int)
self.color_buffer[:] = frame[np.ix_(ys, xs)]
self.color_buffer_flag = 1
# ==== .LightEffectManager.render_pixel_wave (linha 9296)
# descompilação falhou (ParserError: Parse error at or near `LOAD_FAST' instruction at offset 0
); desmontagem:
# # pydisasm version 6.1.7
# # Python bytecode 3.8.0 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       render_pixel_wave
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  44
# # Stack size:        13
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        9296
# # Constants:
# #    0: None
# #    1: 'pixel_wave'
# #    2: 'speed'
# #    3: 1
# #    4: 10
# #    5: 'colors'
# #    6: ('dtype',)
# #    7: 100
# #    8: 2
# #    9: 1.4
# #   10: 0.05
# #   11: 0.02
# #   12: 'pixel_wave_buffer'
# #   13: 3
# #   14: 0
# #   15: 4
# #   16: -1
# # Names:
# #    0: effects
# #    1: np
# #    2: clip
# #    3: array
# #    4: uint8
# #    5: len
# #    6: pi
# #    7: hasattr
# #    8: full
# #    9: pixel_wave_buffer
# #   10: animation_phase
# #   11: arange
# #   12: sqrt
# #   13: arctan2
# #   14: sin
# #   15: cos
# #   16: random
# #   17: randint
# #   18: range
# #   19: append
# #   20: int
# #   21: abs
# #   22: width
# #   23: height
# #   24: linspace
# #   25: ix_
# #   26: color_buffer
# #   27: color_buffer_flag
# # Varnames:
# #	self, effect, speed, colors, color_count, LOW_RES, PIXEL_STEP, ROTATE_STEP, WAVE_INTENSITY, DIST_COEFFICIENT, ANIM_VELOCITY, CENTER_POS, frame_buffer, x_coords, y_coords, y, x, dx, dy, distance, angle, wave_value, current_color, base_size, center_x, center_y, points, i, px, py, x1, y1, x2, y2, dx_line, dy_line, sx_line, sy_line, err_line, e2, target_w, target_h, x_map, y_map
# # Positional arguments:
# #	self
# # Local variables:
# #    1: effect
# #    2: speed
# #    3: colors
# #    4: color_count
# #    5: LOW_RES
# #    6: PIXEL_STEP
# #    7: ROTATE_STEP
# #    8: WAVE_INTENSITY
# #    9: DIST_COEFFICIENT
# #   10: ANIM_VELOCITY
# #   11: CENTER_POS
# #   12: frame_buffer
# #   13: x_coords
# #   14: y_coords
# #   15: y
# #   16: x
# #   17: dx
# #   18: dy
# #   19: distance
# #   20: angle
# #   21: wave_value
# #   22: current_color
# #   23: base_size
# #   24: center_x
# #   25: center_y
# #   26: points
# #   27: i
# #   28: px
# #   29: py
# #   30: x1
# #   31: y1
# #   32: x2
# #   33: y2
# #   34: dx_line
# #   35: dy_line
# #   36: sx_line
# #   37: sy_line
# #   38: err_line
# #   39: e2
# #   40: target_w
# #   41: target_h
# #   42: x_map
# #   43: y_map
# 
# # Method Name:       render_pixel_wave
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  44
# # Stack size:        13
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        9296
# # Constants:
# #    0: None
# #    1: 'pixel_wave'
# #    2: 'speed'
# #    3: 1
# #    4: 10
# #    5: 'colors'
# #    6: ('dtype',)
# #    7: 100
# #    8: 2
# #    9: 1.4
# #   10: 0.05
# #   11: 0.02
# #   12: 'pixel_wave_buffer'
# #   13: 3
# #   14: 0
# #   15: 4
# #   16: -1
# # Names:
# #    0: effects
# #    1: np
# #    2: clip
# #    3: array
# #    4: uint8
# #    5: len
# #    6: pi
# #    7: hasattr
# #    8: full
# #    9: pixel_wave_buffer
# #   10: animation_phase
# #   11: arange
# #   12: sqrt
# #   13: arctan2
# #   14: sin
# #   15: cos
# #   16: random
# #   17: randint
# #   18: range
# #   19: append
# #   20: int
# #   21: abs
# #   22: width
# #   23: height
# #   24: linspace
# #   25: ix_
# #   26: color_buffer
# #   27: color_buffer_flag
# # Varnames:
# #	self, effect, speed, colors, color_count, LOW_RES, PIXEL_STEP, ROTATE_STEP, WAVE_INTENSITY, DIST_COEFFICIENT, ANIM_VELOCITY, CENTER_POS, frame_buffer, x_coords, y_coords, y, x, dx, dy, distance, angle, wave_value, current_color, base_size, center_x, center_y, points, i, px, py, x1, y1, x2, y2, dx_line, dy_line, sx_line, sy_line, err_line, e2, target_w, target_h, x_map, y_map
# # Positional arguments:
# #	self
# # Local variables:
# #    1: effect
# #    2: speed
# #    3: colors
# #    4: color_count
# #    5: LOW_RES
# #    6: PIXEL_STEP
# #    7: ROTATE_STEP
# #    8: WAVE_INTENSITY
# #    9: DIST_COEFFICIENT
# #   10: ANIM_VELOCITY
# #   11: CENTER_POS
# #   12: frame_buffer
# #   13: x_coords
# #   14: y_coords
# #   15: y
# #   16: x
# #   17: dx
# #   18: dy
# #   19: distance
# #   20: angle
# #   21: wave_value
# #   22: current_color
# #   23: base_size
# #   24: center_x
# #   25: center_y
# #   26: points
# #   27: i
# #   28: px
# #   29: py
# #   30: x1
# #   31: y1
# #   32: x2
# #   33: y2
# #   34: dx_line
# #   35: dy_line
# #   36: sx_line
# #   37: sy_line
# #   38: err_line
# #   39: e2
# #   40: target_w
# #   41: target_h
# #   42: x_map
# #   43: y_map
# 9298:           0 LOAD_FAST            (self)
#                2 LOAD_ATTR            (effects)
#                4 LOAD_CONST           ("pixel_wave")
#                6 BINARY_SUBSCR
#                8 STORE_FAST           (effect)
# 
# 9299:          10 LOAD_GLOBAL          (np)
#               12 LOAD_METHOD          (clip)
#               14 LOAD_FAST            (effect)
#               16 LOAD_CONST           ("speed")
#               18 BINARY_SUBSCR
#               20 LOAD_CONST           (1)
#               22 LOAD_CONST           (10)
#               24 CALL_METHOD          (3 positional arguments)
#               26 STORE_FAST           (speed)
# 
# 9300:          28 LOAD_GLOBAL          (np)
#               30 LOAD_ATTR            (array)
#               32 LOAD_FAST            (effect)
#               34 LOAD_CONST           ("colors")
#               36 BINARY_SUBSCR
#               38 LOAD_GLOBAL          (np)
#               40 LOAD_ATTR            (uint8)
#               42 LOAD_CONST           (('dtype',))
#               44 CALL_FUNCTION_KW     (2 total positional and keyword args)
#               46 STORE_FAST           (colors)
# 
# 9301:          48 LOAD_GLOBAL          (len)
#               50 LOAD_FAST            (colors)
#               52 CALL_FUNCTION        (1 positional argument)
#               54 STORE_FAST           (color_count)
# 
# 9306:          56 LOAD_CONST           (100)
#               58 STORE_FAST           (LOW_RES)
# 
# 9307:          60 LOAD_CONST           (10)
#               62 STORE_FAST           (PIXEL_STEP)
# 
# 9308:          64 LOAD_GLOBAL          (np)
#               66 LOAD_ATTR            (pi)
#               68 LOAD_CONST           (2)
#               70 BINARY_TRUE_DIVIDE
#               72 STORE_FAST           (ROTATE_STEP)
# 
# 9309:          74 LOAD_CONST           (1.4)
#               76 STORE_FAST           (WAVE_INTENSITY)
# 
# 9310:          78 LOAD_CONST           (0.05)
#               80 STORE_FAST           (DIST_COEFFICIENT)
# 
# 9311:          82 LOAD_FAST            (speed)
#               84 LOAD_CONST           (0.02)
#               86 BINARY_MULTIPLY
#               88 STORE_FAST           (ANIM_VELOCITY)
# 
# 9312:          90 LOAD_FAST            (LOW_RES)
#               92 LOAD_CONST           (2)
#               94 BINARY_FLOOR_DIVIDE
#               96 STORE_FAST           (CENTER_POS)
# 
# 9317:          98 LOAD_GLOBAL          (hasattr)
#              100 LOAD_FAST            (self)
#              102 LOAD_CONST           ("pixel_wave_buffer")
#              104 CALL_FUNCTION        (2 positional arguments)
#              106 POP_JUMP_IF_TRUE     (to 144)
# 
# 9318:         108 LOAD_GLOBAL          (np)
#              110 LOAD_ATTR            (full)
#              112 LOAD_FAST            (LOW_RES)
#              114 LOAD_FAST            (LOW_RES)
#              116 LOAD_CONST           (3)
#              118 BUILD_TUPLE          3
#              120 LOAD_FAST            (colors)
#              122 LOAD_CONST           (0)
#              124 BINARY_SUBSCR
#              126 LOAD_GLOBAL          (np)
#              128 LOAD_ATTR            (uint8)
#              130 LOAD_CONST           (('dtype',))
#              132 CALL_FUNCTION_KW     (3 total positional and keyword args)
#              134 LOAD_FAST            (self)
#              136 STORE_ATTR           (pixel_wave_buffer)
# 
# 9319:         138 LOAD_CONST           (0)
#              140 LOAD_FAST            (self)
#              142 STORE_ATTR           (animation_phase)
# 
# 9322:     >>  144 LOAD_FAST            (self)
#              146 LOAD_ATTR            (pixel_wave_buffer)
#              148 STORE_FAST           (frame_buffer)
# 
# 9323:         150 LOAD_FAST            (colors)
#              152 LOAD_CONST           (0)
#              154 BINARY_SUBSCR
#              156 LOAD_FAST            (frame_buffer)
#              158 LOAD_CONST           (None)
#              160 LOAD_CONST           (None)
#              162 BUILD_SLICE          2
#              164 STORE_SUBSCR
# 
# 9324:         166 LOAD_FAST            (self)
#              168 DUP_TOP
#              170 LOAD_ATTR            (animation_phase)
#              172 LOAD_FAST            (ANIM_VELOCITY)
#              174 INPLACE_ADD
#              176 ROT_TWO
#              178 STORE_ATTR           (animation_phase)
# 
# 9329:         180 LOAD_GLOBAL          (np)
#              182 LOAD_METHOD          (arange)
#              184 LOAD_CONST           (0)
#              186 LOAD_FAST            (LOW_RES)
#              188 LOAD_FAST            (PIXEL_STEP)
#              190 CALL_METHOD          (3 positional arguments)
#              192 STORE_FAST           (x_coords)
# 
# 9330:         194 LOAD_GLOBAL          (np)
#              196 LOAD_METHOD          (arange)
#              198 LOAD_CONST           (0)
#              200 LOAD_FAST            (LOW_RES)
#              202 LOAD_FAST            (PIXEL_STEP)
#              204 CALL_METHOD          (3 positional arguments)
#              206 STORE_FAST           (y_coords)
# 
# 9333:         208 LOAD_FAST            (y_coords)
#              210 GET_ITER
#          >>  212 EXTENDED_ARG         (256)
#              214 FOR_ITER             (to 710)
#              216 STORE_FAST           (y)
# 
# 9334:         218 LOAD_FAST            (x_coords)
#              220 GET_ITER
#          >>  222 EXTENDED_ARG         (256)
#              224 FOR_ITER             (to 708)
#              226 STORE_FAST           (x)
# 
# 9336:         228 LOAD_FAST            (x)
#              230 LOAD_FAST            (CENTER_POS)
#              232 BINARY_SUBTRACT
#              234 STORE_FAST           (dx)
# 
# 9337:         236 LOAD_FAST            (y)
#              238 LOAD_FAST            (CENTER_POS)
#              240 BINARY_SUBTRACT
#              242 STORE_FAST           (dy)
# 
# 9338:         244 LOAD_GLOBAL          (np)
#              246 LOAD_METHOD          (sqrt)
#              248 LOAD_FAST            (dx)
#              250 LOAD_CONST           (2)
#              252 BINARY_POWER
#              254 LOAD_FAST            (dy)
#              256 LOAD_CONST           (2)
#              258 BINARY_POWER
#              260 BINARY_ADD
#              262 CALL_METHOD          (1 positional argument)
#              264 LOAD_FAST            (DIST_COEFFICIENT)
#              266 BINARY_MULTIPLY
#              268 STORE_FAST           (distance)
# 
# 9339:         270 LOAD_GLOBAL          (np)
#              272 LOAD_METHOD          (arctan2)
#              274 LOAD_FAST            (dy)
#              276 LOAD_FAST            (dx)
#              278 CALL_METHOD          (2 positional arguments)
#              280 STORE_FAST           (angle)
# 
# 9340:         282 LOAD_GLOBAL          (np)
#              284 LOAD_METHOD          (sin)
#              286 LOAD_GLOBAL          (np)
#              288 LOAD_METHOD          (cos)
#              290 LOAD_FAST            (angle)
#              292 CALL_METHOD          (1 positional argument)
#              294 LOAD_FAST            (distance)
#              296 BINARY_ADD
#              298 LOAD_FAST            (self)
#              300 LOAD_ATTR            (animation_phase)
#              302 BINARY_SUBTRACT
#              304 CALL_METHOD          (1 positional argument)
#              306 LOAD_CONST           (1)
#              308 BINARY_ADD
#              310 STORE_FAST           (wave_value)
# 
# 9341:         312 LOAD_FAST            (wave_value)
#              314 LOAD_FAST            (WAVE_INTENSITY)
#              316 INPLACE_MULTIPLY
#              318 STORE_FAST           (wave_value)
# 
# 9344:         320 LOAD_FAST            (colors)
#              322 LOAD_GLOBAL          (np)
#              324 LOAD_ATTR            (random)
#              326 LOAD_METHOD          (randint)
#              328 LOAD_CONST           (1)
#              330 LOAD_FAST            (color_count)
#              332 CALL_METHOD          (2 positional arguments)
#              334 BINARY_SUBSCR
#              336 STORE_FAST           (current_color)
# 
# 9345:         338 LOAD_FAST            (PIXEL_STEP)
#              340 LOAD_FAST            (wave_value)
#              342 BINARY_MULTIPLY
#              344 LOAD_CONST           (2)
#              346 BINARY_TRUE_DIVIDE
#              348 STORE_FAST           (base_size)
# 
# 9346:         350 LOAD_FAST            (x)
#              352 LOAD_FAST            (PIXEL_STEP)
#              354 LOAD_CONST           (2)
#              356 BINARY_TRUE_DIVIDE
#              358 BINARY_ADD
#              360 STORE_FAST           (center_x)
# 
# 9347:         362 LOAD_FAST            (y)
#              364 LOAD_FAST            (PIXEL_STEP)
#              366 LOAD_CONST           (2)
#              368 BINARY_TRUE_DIVIDE
#              370 BINARY_ADD
#              372 STORE_FAST           (center_y)
# 
# 9350:         374 BUILD_LIST           0
#              376 STORE_FAST           (points)
# 
# 9351:         378 LOAD_GLOBAL          (range)
#              380 LOAD_CONST           (4)
#              382 CALL_FUNCTION        (1 positional argument)
#              384 GET_ITER
#          >>  386 FOR_ITER             (to 488)
#              388 STORE_FAST           (i)
# 
# 9352:         390 LOAD_GLOBAL          (np)
#              392 LOAD_METHOD          (cos)
#              394 LOAD_FAST            (ROTATE_STEP)
#              396 LOAD_FAST            (i)
#              398 BINARY_MULTIPLY
#              400 CALL_METHOD          (1 positional argument)
#              402 LOAD_FAST            (base_size)
#              404 BINARY_MULTIPLY
#              406 LOAD_FAST            (center_x)
#              408 BINARY_ADD
#              410 STORE_FAST           (px)
# 
# 9353:         412 LOAD_GLOBAL          (np)
#              414 LOAD_METHOD          (sin)
#              416 LOAD_FAST            (ROTATE_STEP)
#              418 LOAD_FAST            (i)
#              420 BINARY_MULTIPLY
#              422 CALL_METHOD          (1 positional argument)
#              424 LOAD_FAST            (base_size)
#              426 BINARY_MULTIPLY
#              428 LOAD_FAST            (center_y)
#              430 BINARY_ADD
#              432 STORE_FAST           (py)
# 
# 9354:         434 LOAD_FAST            (points)
#              436 LOAD_METHOD          (append)
#              438 LOAD_GLOBAL          (int)
#              440 LOAD_GLOBAL          (np)
#              442 LOAD_METHOD          (clip)
#              444 LOAD_FAST            (px)
#              446 LOAD_CONST           (0)
#              448 LOAD_FAST            (LOW_RES)
#              450 LOAD_CONST           (1)
#              452 BINARY_SUBTRACT
#              454 CALL_METHOD          (3 positional arguments)
#              456 CALL_FUNCTION        (1 positional argument)
#              458 LOAD_GLOBAL          (int)
#              460 LOAD_GLOBAL          (np)
#              462 LOAD_METHOD          (clip)
#              464 LOAD_FAST            (py)
#              466 LOAD_CONST           (0)
#              468 LOAD_FAST            (LOW_RES)
#              470 LOAD_CONST           (1)
#              472 BINARY_SUBTRACT
#              474 CALL_METHOD          (3 positional arguments)
#              476 CALL_FUNCTION        (1 positional argument)
#              478 BUILD_TUPLE          2
#              480 CALL_METHOD          (1 positional argument)
#              482 POP_TOP
#              484 EXTENDED_ARG         (256)
#              486 JUMP_ABSOLUTE        (to 386)
# 
# 9359:     >>  488 LOAD_GLOBAL          (range)
#              490 LOAD_CONST           (4)
#              492 CALL_FUNCTION        (1 positional argument)
#              494 GET_ITER
#          >>  496 FOR_ITER             (to 706)
#              498 STORE_FAST           (i)
# 
# 9360:         500 LOAD_FAST            (points)
#              502 LOAD_FAST            (i)
#              504 BINARY_SUBSCR
#              506 UNPACK_SEQUENCE      2
#              508 STORE_FAST           (x1)
#              510 STORE_FAST           (y1)
# 
# 9361:         512 LOAD_FAST            (points)
#              514 LOAD_FAST            (i)
#              516 LOAD_CONST           (1)
#              518 BINARY_ADD
#              520 LOAD_CONST           (4)
#              522 BINARY_MODULO
#              524 BINARY_SUBSCR
#              526 UNPACK_SEQUENCE      2
#              528 STORE_FAST           (x2)
#              530 STORE_FAST           (y2)
# 
# 9364:         532 LOAD_GLOBAL          (abs)
#              534 LOAD_FAST            (x2)
#              536 LOAD_FAST            (x1)
#              538 BINARY_SUBTRACT
#              540 CALL_FUNCTION        (1 positional argument)
#              542 STORE_FAST           (dx_line)
# 
# 9365:         544 LOAD_GLOBAL          (abs)
#              546 LOAD_FAST            (y2)
#              548 LOAD_FAST            (y1)
#              550 BINARY_SUBTRACT
#              552 CALL_FUNCTION        (1 positional argument)
#              554 STORE_FAST           (dy_line)
# 
# 9366:         556 LOAD_FAST            (x1)
#              558 LOAD_FAST            (x2)
#              560 COMPARE_OP           (<)
#              562 EXTENDED_ARG         (512)
#              564 POP_JUMP_IF_FALSE    (to 570)
#              566 LOAD_CONST           (1)
#              568 JUMP_FORWARD         (to 572)
#          >>  570 LOAD_CONST           (-1)
#          >>  572 STORE_FAST           (sx_line)
# 
# 9367:         574 LOAD_FAST            (y1)
#              576 LOAD_FAST            (y2)
#              578 COMPARE_OP           (<)
#              580 EXTENDED_ARG         (512)
#              582 POP_JUMP_IF_FALSE    (to 588)
#              584 LOAD_CONST           (1)
#              586 JUMP_FORWARD         (to 590)
#          >>  588 LOAD_CONST           (-1)
#          >>  590 STORE_FAST           (sy_line)
# 
# 9368:         592 LOAD_FAST            (dx_line)
#              594 LOAD_FAST            (dy_line)
#              596 BINARY_SUBTRACT
#              598 STORE_FAST           (err_line)
# 
# 9371:     >>  600 LOAD_FAST            (current_color)
#              602 LOAD_FAST            (frame_buffer)
#              604 LOAD_FAST            (y1)
#              606 LOAD_FAST            (x1)
#              608 BUILD_TUPLE          2
#              610 STORE_SUBSCR
# 
# 9372:         612 LOAD_FAST            (x1)
#              614 LOAD_FAST            (x2)
#              616 COMPARE_OP           (==)
#              618 EXTENDED_ARG         (512)
#              620 POP_JUMP_IF_FALSE    (to 636)
#              622 LOAD_FAST            (y1)
#              624 LOAD_FAST            (y2)
#              626 COMPARE_OP           (==)
#              628 EXTENDED_ARG         (512)
#              630 POP_JUMP_IF_FALSE    (to 636)
# 
# 9373:         632 EXTENDED_ARG         (256)
#              634 JUMP_ABSOLUTE        (to 496)
# 
# 9374:     >>  636 LOAD_CONST           (2)
#              638 LOAD_FAST            (err_line)
#              640 BINARY_MULTIPLY
#              642 STORE_FAST           (e2)
# 
# 9375:         644 LOAD_FAST            (e2)
#              646 LOAD_FAST            (dy_line)
#              648 UNARY_NEGATIVE
#              650 COMPARE_OP           (>)
#              652 EXTENDED_ARG         (512)
#              654 POP_JUMP_IF_FALSE    (to 672)
# 
# 9376:         656 LOAD_FAST            (err_line)
#              658 LOAD_FAST            (dy_line)
#              660 INPLACE_SUBTRACT
#              662 STORE_FAST           (err_line)
# 
# 9377:         664 LOAD_FAST            (x1)
#              666 LOAD_FAST            (sx_line)
#              668 INPLACE_ADD
#              670 STORE_FAST           (x1)
# 
# 9378:     >>  672 LOAD_FAST            (e2)
#              674 LOAD_FAST            (dx_line)
#              676 COMPARE_OP           (<)
#              678 EXTENDED_ARG         (512)
#              680 POP_JUMP_IF_FALSE    (to 600)
# 
# 9379:         682 LOAD_FAST            (err_line)
#              684 LOAD_FAST            (dx_line)
#              686 INPLACE_ADD
#              688 STORE_FAST           (err_line)
# 
# 9380:         690 LOAD_FAST            (y1)
#              692 LOAD_FAST            (sy_line)
#              694 INPLACE_ADD
#              696 STORE_FAST           (y1)
#              698 EXTENDED_ARG         (512)
#              700 JUMP_ABSOLUTE        (to 600)
#              702 EXTENDED_ARG         (256)
#              704 JUMP_ABSOLUTE        (to 496)
#          >>  706 JUMP_ABSOLUTE        (to 222)
#          >>  708 JUMP_ABSOLUTE        (to 212)
# 
# 9385:     >>  710 LOAD_FAST            (self)
#              712 LOAD_ATTR            (width)
#              714 STORE_FAST           (target_w)
# 
# 9386:         716 LOAD_FAST            (self)
#              718 LOAD_ATTR            (height)
#              720 STORE_FAST           (target_h)
# 
# 9387:         722 LOAD_GLOBAL          (np)
#              724 LOAD_ATTR            (linspace)
#              726 LOAD_CONST           (0)
#              728 LOAD_FAST            (LOW_RES)
#              730 LOAD_CONST           (1)
#              732 BINARY_SUBTRACT
#              734 LOAD_FAST            (target_w)
#              736 LOAD_GLOBAL          (int)
#              738 LOAD_CONST           (('dtype',))
#              740 CALL_FUNCTION_KW     (4 total positional and keyword args)
#              742 STORE_FAST           (x_map)
# 
# 9388:         744 LOAD_GLOBAL          (np)
#              746 LOAD_ATTR            (linspace)
#              748 LOAD_CONST           (0)
#              750 LOAD_FAST            (LOW_RES)
#              752 LOAD_CONST           (1)
#              754 BINARY_SUBTRACT
#              756 LOAD_FAST            (target_h)
#              758 LOAD_GLOBAL          (int)
#              760 LOAD_CONST           (('dtype',))
#              762 CALL_FUNCTION_KW     (4 total positional and keyword args)
#              764 STORE_FAST           (y_map)
# 
# 9391:         766 LOAD_FAST            (frame_buffer)
#              768 LOAD_GLOBAL          (np)
#              770 LOAD_METHOD          (ix_)
#              772 LOAD_FAST            (y_map)
#              774 LOAD_FAST            (x_map)
#              776 CALL_METHOD          (2 positional arguments)
#              778 BINARY_SUBSCR
#              780 LOAD_FAST            (self)
#              782 LOAD_ATTR            (color_buffer)
#              784 LOAD_CONST           (None)
#              786 LOAD_CONST           (None)
#              788 BUILD_SLICE          2
#              790 STORE_SUBSCR
# 
# 9392:         792 LOAD_CONST           (1)
#              794 LOAD_FAST            (self)
#              796 STORE_ATTR           (color_buffer_flag)
#              798 LOAD_CONST           (None)
#              800 RETURN_VALUE
# 
# 
# ==== .LightEffectManager.render_planetary_orbit (linha 9398)
effect = self.effects["planetary_orbit"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.uint8))
color_num = len(colors)
LOW_RES = 100
CX = LOW_RES // 2
CY = LOW_RES // 2
STAR_SIZE = 7
ORBIT_WIDTH = 1
PLANET_NUM = 4
PLANET_SIZE = 3
ORBIT_STEP = 10
ORBIT_MIN = 15
ANIM_SPEED = speed * 0.01
if not hasattr(self, "orbit_buf"):
    self.orbit_buf = np.full((LOW_RES, LOW_RES, 3), (colors[0]), dtype=(np.uint8))
    self.phase = 0
    self.orbit_radii = [ORBIT_MIN + i * ORBIT_STEP for i in range(PLANET_NUM)]
    self.planet_rates = [1.0 + i * 0.22 for i in range(PLANET_NUM)]
buf = self.orbit_buf
buf[:] = colors[0]
self.phase += ANIM_SPEED
Y, X = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
star_mask = (X - CX) ** 2 + (Y - CY) ** 2 < STAR_SIZE ** 2
buf[star_mask] = colors[1]
for i in range(PLANET_NUM):
    r = self.orbit_radii[i]
    rate = self.planet_rates[i]
    angle = self.phase * rate
    orbit_inner = (X - CX) ** 2 + (Y - CY) ** 2 < (r - ORBIT_WIDTH) ** 2
    orbit_outer = (X - CX) ** 2 + (Y - CY) ** 2 < (r + ORBIT_WIDTH) ** 2
    orbit_mask = ~orbit_inner & orbit_outer
    buf[orbit_mask] = colors[2 + i % (color_num - 2)] // 3
    px = CX + np.sin(angle) * r
    py = CY + np.cos(angle) * r
    planet_mask = (X - px) ** 2 + (Y - py) ** 2 < PLANET_SIZE ** 2
    buf[planet_mask] = colors[2 + i % (color_num - 2)]

target_w, target_h = self.width, self.height
x_map = np.linspace(0, (LOW_RES - 1), target_w, dtype=int)
y_map = np.linspace(0, (LOW_RES - 1), target_h, dtype=int)
self.color_buffer[:] = buf[np.ix_(y_map, x_map)]
self.color_buffer_flag = 1
# ==== .LightEffectManager.render_plasma_flow (linha 9481)
effect = self.effects["plasma_flow"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
color_outer = colors[1]
color_mid = colors[2]
color_center = colors[3]
LOW_RES = 100
NOISE_SCALE = 14.0
ANIM_SPEED = speed * 0.0025
LEVEL1 = 0.35
LEVEL2 = 0.55
LEVEL3 = 0.75
if not hasattr(self, "plasma_buf"):
    self.plasma_buf = np.full((LOW_RES, LOW_RES, 3), bg_color, dtype=(np.uint8))
    self.plasma_time = 0.0
buf = self.plasma_buf
buf[:] = bg_color
self.plasma_time += ANIM_SPEED
Y, X = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
xs = X / NOISE_SCALE
ys = Y / NOISE_SCALE
n1 = np.sin(xs + self.plasma_time) * np.cos(ys + self.plasma_time * 1.17)
n2 = np.sin((xs + ys + self.plasma_time) * 0.75)
val = (n1 + n2) * 0.5 + 0.5
mask_center = val > LEVEL3
mask_mid = (val > LEVEL2) & (val <= LEVEL3)
mask_outer = (val > LEVEL1) & (val <= LEVEL2)
buf[mask_outer] = color_outer
buf[mask_mid] = color_mid
buf[mask_center] = color_center
target_w, target_h = self.width, self.height
x_map = np.linspace(0, (LOW_RES - 1), target_w, dtype=int)
y_map = np.linspace(0, (LOW_RES - 1), target_h, dtype=int)
self.color_buffer[:] = buf[np.ix_(y_map, x_map)]
self.color_buffer_flag = 1
# ==== .LightEffectManager.render_poison (linha 9543)
effect = self.effects["poison_max"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
poison1 = colors[1]
poison2 = colors[2]
poison3 = colors[3]
color_list = [poison1, poison2, poison3]
LOW_RES = 100
MAX_PARTICLES = 30
MAX_RADIUS = 12
BASE_SPEED = speed * 0.12
GROW_SPEED = speed * 0.02
ALPHA = 100
if not hasattr(self, "poison_buf_mimai"):
    self.poison_buf_mimai = np.full((LOW_RES, LOW_RES, 3), bg_color, dtype=(np.uint8))
    self.particles_mimai = []
    for _ in range(MAX_PARTICLES):
        x = np.random.uniform(0, LOW_RES)
        y = np.random.uniform(0, LOW_RES)
        dx = np.random.uniform(-0.3, 0.3)
        dy = np.random.uniform(0.5, 2.0) if np.random.random() > 0.5 else np.random.uniform(-2.0, -0.5)
        r = 1.0
        w = np.random.uniform(1, 4)
        c = np.random.randint(0, 3)
        self.particles_mimai.append([x,y,dx,dy,r,w,c])

buf = self.poison_buf_mimai
buf[:] = bg_color
Y, X = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
alive = []
for p in self.particles_mimai:
    x, y, dx, dy, r, w, c_idx = p
    color = color_list[c_idx]
    dist_sq = (X - x) ** 2 + (Y - y) ** 2
    mask = (dist_sq >= (r - w) ** 2) & (dist_sq <= (r + w) ** 2)
    buf[mask] = color
    x += dx * BASE_SPEED
    y += dy * BASE_SPEED
    r += np.random.uniform(0.1, 0.4) * GROW_SPEED
    if not 0 <= y <= LOW_RES:
        if r <= MAX_RADIUS:
            alive.append([x,y,dx,dy,r,w,c_idx])

while len(alive) < MAX_PARTICLES:
    x = np.random.uniform(0, LOW_RES)
    y = 0.0 if np.random.random() > 0.5 else float(LOW_RES)
    dx = np.random.uniform(-0.3, 0.3)
    dy = np.random.uniform(0.5, 2.0) if y == 0.0 else np.random.uniform(-2.0, -0.5)
    r = 1.0
    w = np.random.uniform(1, 4)
    c = np.random.randint(0, 3)
    alive.append([x,y,dx,dy,r,w,c])

self.particles_mimai = alive
tw, th = self.width, self.height
x_map = np.linspace(0, (LOW_RES - 1), tw, dtype=int)
y_map = np.linspace(0, (LOW_RES - 1), th, dtype=int)
self.color_buffer[:] = buf[np.ix_(y_map, x_map)]
self.color_buffer_flag = 1
# ==== .LightEffectManager.render_rainbow_tap (linha 9640)
effect = self.effects["rainbow_tap"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
wave_colors = [colors[1], colors[2], colors[3]]
LOW_RES = 100
CX = LOW_RES // 2
CY = LOW_RES // 2
WAVE_WIDTH = 2
MAX_WAVES = 6
LIFETIME = 40
EXPAND_SPEED = speed * 0.12
SPAWN_INTERVAL = 12
if not hasattr(self, "ripple_buf_bowei_caihong"):
    self.ripple_buf_bowei_caihong = np.full((LOW_RES, LOW_RES, 3), bg_color, dtype=(np.uint8))
    self.waves_caihong = []
    self.spawn_timer = 0
buf = self.ripple_buf_bowei_caihong
buf[:] = bg_color
self.spawn_timer += 1
Y, X = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
if self.spawn_timer >= SPAWN_INTERVAL:
    if len(self.waves_caihong) < MAX_WAVES:
        self.spawn_timer = 0
        x = np.random.uniform(CX - 15, CX + 15)
        y = np.random.uniform(CY - 15, CY + 15)
        color_idx = np.random.randint(0, 3)
        self.waves_caihong.append({
          'x': x,  'y': y,  'r': 3.0,
          'life': LIFETIME,  'color_idx': color_idx})
alive_waves = []
for wave in self.waves_caihong:
    x = wave["x"]
    y = wave["y"]
    r = wave["r"]
    life = wave["life"]
    c = wave_colors[wave["color_idx"]]
    alpha = life / LIFETIME
    draw_color = (c * alpha).astype(np.uint8)
    dist_sq = (X - x) ** 2 + (Y - y) ** 2
    mask = (dist_sq >= (r - WAVE_WIDTH) ** 2) & (dist_sq <= (r + WAVE_WIDTH) ** 2)
    buf[mask] = draw_color
    wave["r"] += EXPAND_SPEED
    wave["life"] -= 1
    if wave["life"] > 0:
        alive_waves.append(wave)
    self.waves_caihong = alive_waves
    tw, th = self.width, self.height
    x_map = np.linspace(0, (LOW_RES - 1), tw, dtype=int)
    y_map = np.linspace(0, (LOW_RES - 1), th, dtype=int)
    self.color_buffer[:] = buf[np.ix_(y_map, x_map)]
    self.color_buffer_flag = 1
# ==== .LightEffectManager.render_resident_evil (linha 9733)
effect = self.effects["resident_evil"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg_color = colors[0]
blood_color = colors[1]
umbrella_color = colors[2]
LOW_RES = 100
CX = LOW_RES // 2
CY = LOW_RES // 2
MAX_BLOOD = 12
BLOOD_RADIUS = 2
ROT_SPEED = speed * 0.0025
FALL_SPEED = speed * 0.15
MAX_RADIUS = 35
if not hasattr(self, "evil_buf"):
    self.evil_buf = np.full((LOW_RES, LOW_RES, 3), bg_color, dtype=(np.uint8))
    self.rotation = 0.0
    self.blood_list = []
    for _ in range(MAX_BLOOD):
        x = np.random.uniform(0, LOW_RES)
        y = np.random.uniform(-50, LOW_RES)
        self.blood_list.append([x, y])

buf = self.evil_buf
buf[:] = bg_color
self.rotation += ROT_SPEED
Y, X = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
cx, cy = CX, CY
rot = self.rotation
r = MAX_RADIUS
for i in range(8):
    ang = rot + i * np.pi / 4
    ang1 = ang - np.pi / 16
    ang2 = ang + np.pi / 16
    x1 = cx + r * np.cos(ang1)
    y1 = cy + r * np.sin(ang1)
    x2 = cx + r * np.cos(ang2)
    y2 = cy + r * np.sin(ang2)
    mask = np.zeros_like(X, dtype=bool)
    edge1 = (X - cx) * (y1 - cy) - (Y - cy) * (x1 - cx) >= -1
    edge2 = (X - x1) * (x2 - x1) + (Y - y1) * (y2 - y1) >= -2
    edge3 = (X - x2) * (cx - x2) + (Y - y2) * (cy - y2) >= -2
    mask = edge1 & edge2 & edge3
    buf[mask] = umbrella_color if i % 2 else blood_color

new_blood = []
for x, y in self.blood_list:
    dist_sq = (X - x) ** 2 + (Y - y) ** 2
    mask = dist_sq < BLOOD_RADIUS ** 2
    buf[mask] = blood_color
    y += FALL_SPEED
    if y < LOW_RES + 10:
        new_blood.append([x, y])
    else:
        new_blood.append([np.random.uniform(0, LOW_RES), -10])

self.blood_list = new_blood
tw, th = self.width, self.height
x_map = np.linspace(0, (LOW_RES - 1), tw, dtype=int)
y_map = np.linspace(0, (LOW_RES - 1), th, dtype=int)
self.color_buffer[:] = buf[np.ix_(y_map, x_map)]
self.color_buffer_flag = 1
# ==== .LightEffectManager.render_rick_morty (linha 9832)
effect = self.effects["rick_morty"]
speed = np.clip(effect["speed"], 1, 10)
colors = np.array((effect["colors"]), dtype=(np.uint8))
bg = colors[0]
portal_main = colors[1]
portal_bright = colors[2]
white_dot = colors[3]
LOW_RES = 100
CX = LOW_RES // 2
CY = LOW_RES // 2
PORTAL_R = 42
OUTER_R = 50
ANIM_SPEED = speed * 0.02
POINT_COUNT = 35
ORBIT_COUNT = 12
if not hasattr(self, "portal_buf"):
    self.portal_buf = np.full((LOW_RES, LOW_RES, 3), bg, dtype=(np.uint8))
    self.t = 0.0
    self.rot = 0.0
    self.points = []
    for _ in range(POINT_COUNT):
        r = np.random.uniform(28, PORTAL_R)
        a = np.random.uniform(0, np.pi * 2)
        self.points.append([r, a])

    self.orbits = []
    for _ in range(ORBIT_COUNT):
        ofs = np.random.uniform(0, np.pi * 4)
        self.orbits.append(ofs)

buf = self.portal_buf
buf[:] = bg
self.t += ANIM_SPEED
self.rot += 0.012
Y, X = np.indices((LOW_RES, LOW_RES), dtype=(np.float32))
dx = X - CX
dy = Y - CY
dist = np.sqrt(dx ** 2 + dy ** 2)
mask_core = dist <= PORTAL_R
mask_outer = (dist > PORTAL_R) & (dist <= OUTER_R)
buf[mask_core] = portal_main
buf[mask_outer] = portal_bright
for r, a in self.points:
    angle = a + self.rot
    x = CX + r * np.cos(angle)
    y = CY + r * np.sin(angle)
    m = (X - x) ** 2 + (Y - y) ** 2 < 9.0
    buf[m] = white_dot

for ofs in self.orbits:
    angle = self.t * 0.7 + ofs
    r = PORTAL_R - 8 + np.cos(self.t * 0.3 + ofs) * 6
    x = CX + r * np.cos(angle)
    y = CY + r * np.sin(angle)
    m = (X - x) ** 2 + (Y - y) ** 2 < 9.0
    buf[m] = white_dot

tw, th = self.width, self.height
x_map = np.linspace(0, (LOW_RES - 1), tw, dtype=int)
y_map = np.linspace(0, (LOW_RES - 1), th, dtype=int)
self.color_buffer[:] = buf[np.ix_(y_map, x_map)]
self.color_buffer_flag = 1