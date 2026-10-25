# decompyle3 version 3.9.3
# Python bytecode version base 3.8.0
# Decompiled from: Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# Embedded file name: pyside2_kittle_ui\fan_speed_kittle.py
import sys
from PySide2.QtWidgets import QApplication, QWidget, QVBoxLayout, QHBoxLayout, QSlider, QLabel, QPushButton, QGroupBox, QSizePolicy
from PySide2.QtGui import QPainter, QColor, QPen, QBrush, QFont, QIcon
from PySide2.QtCore import Qt, QPointF, QMargins, Signal, Slot, QSize, QRect
from PySide2.QtCharts import QtCharts as QtCharts

class DragChartView(QtCharts.QChartView):
    pointDrag = Signal()

    def __init__(self, parent=None):
        super().__init__(parent)
        self.drag_index = -1
        self.dragable_count = 3
        self.points = []
        self.symbol_font = QFont("Arial")
        self.symbol_font.setPixelSize(14)

    def setPoints(self, pts):
        self.points = pts

    def getChartValue(self, pos):
        return self.chart().mapToValue(pos)

    def findPoint(self, val_pt, threshold=4.0):
        for (idx, p) in enumerate(self.points):
            dx = abs(val_pt.x() - p.x())
            dy = abs(val_pt.y() - p.y())
            if not dx < threshold:
                if dy < threshold:
                    return idx
                return -1

    def mousePressEvent(self, event):
        val_pt = self.getChartValue(event.pos())
        self.drag_index = self.findPoint(val_pt)
        if self.drag_index >= self.dragable_count:
            self.drag_index = -1
        super().mousePressEvent(event)

    def mouseMoveEvent(self, event):
        if self.drag_index == -1:
            super().mouseMoveEvent(event)
            return
        val_pt = self.getChartValue(event.pos())
        idx = self.drag_index
        new_x = max(20, min(100, val_pt.x()))
        new_y = max(20, min(100, val_pt.y()))
        if idx == 0:
            new_x = min(new_x, self.points[1].x() - 2)
        elif idx == 1:
            new_x = max(self.points[0].x() + 2, min(new_x, self.points[2].x() - 2))
        elif idx == 2:
            new_x = max(self.points[1].x() + 2, min(new_x, self.points[3].x() - 2))
        self.points[idx] = QPointF(new_x, new_y)
        self.pointDrag.emit()
        super().mouseMoveEvent(event)

    def mouseReleaseEvent(self, event):
        self.drag_index = -1
        super().mouseReleaseEvent(event)

    def paintEvent(self, event):
        super().paintEvent(event)
        painter = QPainter(self.viewport())
        painter.setRenderHint(QPainter.Antialiasing)
        painter.setPen(Qt.white)
        painter.setFont(self.symbol_font)
        chart = self.chart()
        x_100_pos = chart.mapToPosition(QPointF(100, 0))
        painter.drawText(int(x_100_pos.x()) + 22, int(x_100_pos.y()) + 14, "℃")
        y_100_pos = chart.mapToPosition(QPointF(0, 100))
        painter.drawText(int(y_100_pos.x()) - 24, int(y_100_pos.y()) - 10, "≋")
        painter.end()


class FanControlWidget(QWidget):
    speedChanged = Signal(int)
    pointMoved = Signal()

    def __init__(self, height=400, parent=None):
        super().__init__(parent)
        self.points = [
         QPointF(20, 20),
         QPointF(39, 30),
         QPointF(60, 78),
         QPointF(100, 100)]
        self.temp_val = 50
        self.out_speed = 0
        self._height = height
        self.init_ui()
        self.setup_chart()
        self.setFixedHeight(height)

    def init_ui(self):
        layout = QVBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)
        self.chart_view = DragChartView()
        self.chart_view.setRenderHint(QPainter.Antialiasing)
        layout.addWidget(self.chart_view)
        self.chart_view.setSizePolicy(QSizePolicy.Expanding, QSizePolicy.Expanding)
        self.chart_view.pointDrag.connect(self.on_point_drag)

    def setup_chart(self):
        self.chart = QtCharts.QChart()
        self.chart.setBackgroundBrush(Qt.transparent)
        self.chart.setMargins(QMargins(8, 28, 28, 8))
        self.chart.legend().setVisible(False)
        self.red_upper = QtCharts.QLineSeries()
        self.red_lower = QtCharts.QLineSeries()
        self.red_area = QtCharts.QAreaSeries(self.red_upper, self.red_lower)
        self.red_area.setPen(QPen(Qt.transparent))
        self.green_upper = QtCharts.QLineSeries()
        self.green_lower = QtCharts.QLineSeries()
        self.green_area = QtCharts.QAreaSeries(self.green_upper, self.green_lower)
        self.green_area.setPen(QPen(Qt.transparent))
        self.line_series = QtCharts.QLineSeries()
        line_pen = QPen(QColor("#FBE900"))
        line_pen.setWidth(3)
        self.line_series.setPen(line_pen)
        self.scatter_white = QtCharts.QScatterSeries()
        self.scatter_white.setMarkerSize(10)
        self.scatter_white.setBrush(Qt.white)
        self.scatter_white.setPen(QPen(Qt.white, 2))
        self.scatter_red_fixed = QtCharts.QScatterSeries()
        self.scatter_red_fixed.setMarkerSize(10)
        self.scatter_red_fixed.setBrush(QColor(255, 0, 0))
        self.scatter_red_fixed.setPen(QPen(Qt.red, 2))
        self.x_axis = QtCharts.QValueAxis()
        self.x_axis.setRange(0, 100)
        self.x_axis.setTickCount(11)
        self.x_axis.setLabelFormat("%d")
        self.x_axis.setLabelsColor(Qt.white)
        self.x_axis.setGridLineColor(QColor("#444444"))
        self.x_axis.setLinePenColor(Qt.white)
        self.y_axis = QtCharts.QValueAxis()
        self.y_axis.setRange(0, 100)
        self.y_axis.setTickCount(11)
        self.y_axis.setLabelFormat("%d")
        self.y_axis.setLabelsColor(Qt.white)
        self.y_axis.setGridLineColor(QColor("#444444"))
        self.y_axis.setLinePenColor(Qt.white)
        self.chart.addSeries(self.red_area)
        self.chart.addSeries(self.green_area)
        self.chart.addSeries(self.line_series)
        self.chart.addSeries(self.scatter_white)
        self.chart.addSeries(self.scatter_red_fixed)
        self.chart.addAxis(self.x_axis, Qt.AlignBottom)
        self.chart.addAxis(self.y_axis, Qt.AlignLeft)
        for ser in (
         self.red_area, self.green_area, self.line_series, self.scatter_white, self.scatter_red_fixed):
            ser.attachAxis(self.x_axis)
            ser.attachAxis(self.y_axis)

        self.chart_view.setChart(self.chart)
        self.chart_view.setPoints(self.points)
        self.refresh_curve()

    def calc_speed_by_temp(self, temp_x):
        x = max(0, min(100, temp_x))
        pts = self.points
        if x <= pts[0].x():
            return pts[0].y()
        if x <= pts[1].x():
            x0, y0 = pts[0].x(), pts[0].y()
            x1, y1 = pts[1].x(), pts[1].y()
            k = (y1 - y0) / (x1 - x0)
            return y0 + k * (x - x0)
        if x <= pts[2].x():
            x1, y1 = pts[1].x(), pts[1].y()
            x2, y2 = pts[2].x(), pts[2].y()
            k = (y2 - y1) / (x2 - x1)
            return y1 + k * (x - x1)
        x2, y2 = pts[2].x(), pts[2].y()
        x3, y3 = pts[3].x(), pts[3].y()
        k = (y3 - y2) / (x3 - x2)
        return y2 + k * (x - x2)

    def refresh_curve(self):
        self.chart_view.setPoints(self.points)
        self.line_series.clear()
        self.scatter_white.clear()
        self.scatter_red_fixed.clear()
        for p in self.points:
            self.line_series.append(p)

        for (i, p) in enumerate(self.points):
            if i < 3:
                self.scatter_white.append(p)
            else:
                self.scatter_red_fixed.append(p)

        self.out_speed = round(self.calc_speed_by_temp(self.temp_val))
        self.speedChanged.emit(self.out_speed)
        temp_x = self.temp_val
        y_temp = self.out_speed
        interp_pt = QPointF(temp_x, y_temp)
        self.red_upper.clear()
        self.red_lower.clear()
        self.red_upper.append(QPointF(0, self.points[0].y()))
        added = False
        for p in self.points:
            if p.x() < temp_x:
                self.red_upper.append(p)
            if not added:
                self.red_upper.append(interp_pt)
                added = True

        if not added:
            self.red_upper.append(interp_pt)
        self.red_lower.append(QPointF(0, 0))
        self.red_lower.append(QPointF(temp_x, 0))
        self.red_area.setBrush(QColor(255, 0, 0, 120))
        self.green_upper.clear()
        self.green_lower.clear()
        self.green_upper.append(interp_pt)
        for p in self.points:
            if p.x() > temp_x:
                self.green_upper.append(p)
            self.green_lower.append(QPointF(temp_x, 0))
            self.green_lower.append(QPointF(100, 0))
            self.green_area.setBrush(QColor(0, 255, 0, 120))

    def set_temperature(self, temp: int):
        self.temp_val = max(0, min(100, temp))
        self.refresh_curve()

    def on_point_drag(self):
        self.refresh_curve()
        self.pointMoved.emit()

    def get_speed(self):
        return self.out_speed

    def get_temp(self):
        return self.temp_val

    def get_curve_points(self):
        return [(
         round(p.x(), 1), round(p.y(), 1)) for p in self.points[:3]]

    def set_curve_points(self, point_list):
        new_pts = []
        for (x, y) in point_list[:3]:
            nx = max(20, min(100, x))
            ny = max(20, min(100, y))
            new_pts.append(QPointF(nx, ny))

        new_pts.append(QPointF(100, 100))
        new_pts.sort(key=(lambda p: p.x()))
        if new_pts[-1].x() != 100:
            new_pts[-1] = QPointF(100, 100)
        self.points = new_pts
        self.refresh_curve()


class FanControlDemo(QWidget):

    def __init__(self):
        super().__init__()
        self.setWindowTitle("3可调白点+1固定红点(100,100) 风扇调速曲线")
        self.resize(1080, 980)
        self.init_ui()

    def init_ui(self):
        layout = QVBoxLayout(self)
        title = QLabel("3个白色可拖拽拐点 | 末端红色固定点(100,100) | 左红右绿")
        title.setAlignment(Qt.AlignCenter)
        title.setStyleSheet("font-size:16px; font-weight:bold; padding:10px; color:#fff;")
        layout.addWidget(title)
        self.fan_ctrl = FanControlWidget(height=440)
        layout.addWidget(self.fan_ctrl, 1)
        info_layout = QHBoxLayout()
        self.temp_label = QLabel(f"当前温度: {self.fan_ctrl.get_temp()} ℃")
        self.speed_label = QLabel(f"对应风扇转速: {self.fan_ctrl.get_speed()} %")
        self.point_label = QLabel("可调三点坐标：")
        for lab in (self.temp_label, self.speed_label, self.point_label):
            lab.setStyleSheet("color:#fff; font-size:13px;")

        info_layout.addWidget(self.temp_label)
        info_layout.addWidget(self.speed_label)
        info_layout.addWidget(self.point_label)
        layout.addLayout(info_layout)
        temp_group = QGroupBox("温度调节滑块 (int 0~100)")
        temp_layout = QVBoxLayout(temp_group)
        slider_row = QHBoxLayout()
        self.temp_slider = QSlider(Qt.Horizontal)
        self.temp_slider.setRange(0, 100)
        self.temp_slider.setValue(50)
        self.temp_slider.setTickInterval(10)
        self.temp_slider.setTickPosition(QSlider.TicksBelow)
        self.temp_slider.valueChanged.connect(self.on_temp_slider)
        slider_row.addWidget(QLabel("0"))
        slider_row.addWidget(self.temp_slider)
        slider_row.addWidget(QLabel("100"))
        temp_layout.addLayout(slider_row)
        btn_row = QHBoxLayout()
        for t in (20, 40, 50, 70, 90, 100):
            btn = QPushButton(f"{t}℃")
            btn.clicked.connect(lambda _, val=t: self.temp_slider.setValue(val))
            btn_row.addWidget(btn)

        temp_layout.addLayout(btn_row)
        layout.addWidget(temp_group)
        self.fan_ctrl.speedChanged.connect(self.update_speed_text)
        self.fan_ctrl.pointMoved.connect(self.update_point_text)
        self.update_point_text()
        self.setStyleSheet("\n            QWidget {background:#222; color:#fff;}\n            QGroupBox {border:1px solid #555; border-radius:6px; margin-top:12px; padding-top:18px;}\n            QSlider::groove:horizontal {height:6px; background:#333;}\n            QSlider::handle:horizontal {background:#0099ff; width:16px; height:16px; border-radius:8px; margin:-5px 0;}\n            QSlider::sub-page:horizontal {background:#0099ff;}\n            QPushButton {background:#3a3a3a; border:1px solid #666; padding:6px; border-radius:4px;}\n            QPushButton:hover {background:#505050;}\n        ")

    def on_temp_slider(self, val):
        self.fan_ctrl.set_temperature(int(val))
        self.temp_label.setText(f"当前温度: {val} ℃")

    def update_speed_text(self, spd):
        self.speed_label.setText(f"对应风扇转速: {spd} %")

    def update_point_text(self):
        pts = self.fan_ctrl.get_curve_points()
        txt = "  ".join([f"({int(x)},{int(y)})" for x, y in pts])
        self.point_label.setText(f"可调三点坐标：{txt}")


if __name__ == "__main__":
    app = QApplication(sys.argv)
    font = QFont("Arial", 10)
    app.setFont(font)
    win = FanControlDemo()
    win.show()
    sys.exit(app.exec_())
