# -*- encoding:utf-8 -*-

# index rule:
# 3 ------6------- 2
#
# 7 ------ ------- 5
#
# 0 ------4------- 1

# Stores the triangle values
raw_trigs = [
	[],
	[(0,7,4)],
	[(4,5,1)],
	[(0,5,1),(0,7,5)],

	[(5,6,2)],
	[(0,7,4), (4,7,5), (7,6,5), (5,6,2)],
	[(1,4,6), (1,6,2)],
	[(1,0,7), (1,7,6), (1,6,2)],

	[(7,3,6)],
	[(0,3,4), (4,3,6)],
	[(1,4,5), (4,7,5), (5,7,6), (7,3,6)],
	[(0,5,1), (0,6,5), (0,3,6)],

	[(7,3,5), (5,3,2)],
	[(0,3,4), (4,3,5), (5,3,2)],
	[(7,3,2), (4,7,2), (1,4,2)],
	[(0,2,1), (0,3,2)],
]

index_to_pos = [
	[0,0],
	[2,0],
	[2,2],
	[0,2],
	[1,0],
	[2,1],
	[1,2],
	[0,1],
]

class Worker(object):
	def __init__(self):
		self.rt_verts = []
		self.rt_trigs = []
		self.rt_flex_verts = []

	def execute(self):
		for case, trig_list in enumerate(raw_trigs):
			self.process_case(case, trig_list)
		worker.format_csharp("MarchingSquareData.txt")

	def process_case(self, case, trig_list_raw):
		vi_list = []
		vpos_list = []
		trig_list = []
		flex_list = []
		for trig_raw in trig_list_raw:
			# print trig_raw
			for vi in trig_raw:
				# record vert index
				if not vi in vi_list:
					vi_list.append(vi)
					vpos_list.append(self.get_vert_pos(vi))

					# record lerp parents
					if vi >= 4:
						parents = self.get_lerp_parents(case, vi)
						flex_list.append(parents)
					else:
						flex_list.append(None)

				# record triangle
				trig_list.append(vi_list.index(vi))
				
		self.rt_verts.append(vpos_list)
		self.rt_trigs.append(trig_list)
		self.rt_flex_verts.append(flex_list)

	def get_vert_pos(self, index):
		return index_to_pos[index]

	def get_lerp_parents(self, case, index):
		if index < 4:
			return
		a, b = (index-4)%4, (index-3)%4
		cell_values = self.get_case_cell_values(case)
		if cell_values[a] > cell_values[b]:
			return b, a
		return a, b

	def get_case_cell_values(self, case):
		return case & 1, case >> 1 & 1, case >> 2 & 1, case >> 3 & 1

	def format_csharp(self, filename):
		msg = ""
		template = "\tnew int[%d] {%s},\n"
		anchor_template = "\tnew int[%d][] {%s},\n"

		with open(filename, 'w') as f:
			# Write cell vertices
			f.write("\n\npublic static int[][] cellVertPos = new int[][]\n{\n")
			msg = ""
			for index in xrange(4):
				msg += template % (2, "%d,%d" % tuple(index_to_pos[index]))
			f.write(msg)
			f.write("};")

			# Write vertices
			f.write("\n\npublic static int[][] vertices = new int[][]\n{\n")
			msg = ""
			for vert_list in self.rt_verts:
				vert_str = ""
				for vert in vert_list:
					vert_str += "%d,%d," % tuple(vert)
				msg += template % (len(vert_list) * 2, vert_str)
			f.write(msg)
			f.write("};")

			# Write triangles
			f.write("\n\npublic static int[][] triangles = new int[][]\n{\n")
			msg = ""
			for trig_list in self.rt_trigs:
				trig_str = ""
				for trig_index in trig_list:
					trig_str += "%d," % trig_index
				msg += template % (len(trig_list), trig_str)
			f.write(msg)
			f.write("};")

			# Write flexible vertices
			f.write("\n\npublic static int[][][] anchors = new int[][][]\n{\n")
			msg = ""
			for flex_list in self.rt_flex_verts:
				flex_str = ""
				for parents in flex_list:
					if parents is not None:
						flex_str += "new int[2]{%d,%d}," % parents
					else:
						flex_str += "null,"
				msg += anchor_template % (len(flex_list), flex_str)
			f.write(msg)
			f.write("};")




worker = Worker()
worker.execute()
# print worker.rt_verts
# print worker.rt_trigs
# print worker.rt_flex_verts
