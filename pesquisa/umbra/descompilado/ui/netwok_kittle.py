# decompyle3 version 3.9.3
# Python bytecode version base 3.8.0
# Decompiled from: Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# Embedded file name: pyside2_kittle_ui\netwok_kittle.py
from PySide2 import QtCore, QtGui, QtWidgets, QtSvg

class NetworkView(QtWidgets.QWidget):

    def __init__(self, parent=None):
        super().__init__(parent)
        self.upload_speed = "0 KB/s"
        self.download_speed = "0 KB/s"
        self.shangchuan = '\n        <svg t="1771904681456" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="7994" width="255" height="255">\n        <path d="M516.281414 464.804704l157.339548 157.340764-31.693413 31.692197-125.646135-125.646134-125.64735 125.646134-31.693414-31.692197z" fill="rgb(251, 233, 0)" p-id="7995"></path>\n        <path d="M493.870886 512.858716h44.821057v228.238311h-44.821057z" fill="rgb(251, 233, 0)" p-id="7996"></path>\n        <path d="M615.000858 816.6348h-0.008515c-42.072195 0-76.299184-34.228206-76.299184-76.3004l0.002433-0.109468H493.873318l-0.001216 0.109468c0 66.893452 54.228006 121.121458 121.121458 121.121458h0.008514v-44.821058z" fill="rgb(251, 233, 0)" p-id="7997"></path>\n        <path d="M802.858863 498.506247a292.347627 292.347627 0 0 0 3.606362-45.781943c0-160.264776-129.920251-290.18381-290.183811-290.183811-156.325145 0-283.755607 123.617328-289.927168 278.428166-98.420231 18.360213-172.926572 104.690313-172.926573 208.435553 0 117.112497 94.937933 212.051646 212.051646 212.051646l0.254209-0.002433v0.002433h105.390909v-44.821058H265.756638v-0.003649c-0.09244 0-0.184879 0.003649-0.278535 0.003649-44.669019 0-86.664586-17.394462-118.249748-48.98084-31.585162-31.585162-48.980841-73.580729-48.98084-118.249748s17.394462-86.664586 48.98084-118.249748c22.566216-22.566216 50.447712-37.88322 80.820212-44.794298a168.56853 168.56853 0 0 1 37.429536-4.186542c2.410728 0 4.811726 0.060816 7.205426 0.161769a249.539565 249.539565 0 0 1-1.764867-29.609873c0-5.121885 0.173932-10.217012 0.484091-15.286596 3.651365-59.787763 28.659934-115.491154 71.381639-158.211643 46.342662-46.342662 107.959757-71.865731 173.498239-71.86573 65.539698 0 127.155576 25.521852 173.498238 71.86573 46.342662 46.342662 71.865731 107.959757 71.865731 173.498239 0 16.313162-1.583637 32.383062-4.674283 48.035767a242.722142 242.722142 0 0 1-15.871641 50.55718c15.072525-5.59503 31.176481-8.522691 47.734122-8.522691 1.11779 0 2.233147 0.014596 3.346071 0.041355 35.320453 0.841687 68.400461 14.992248 93.471062 40.062849 25.861203 25.861203 40.102987 60.24388 40.102987 96.817133 0 36.573253-14.241785 70.95593-40.102987 96.817133-25.861203 25.861203-60.245096 40.102987-96.817133 40.102987l-0.063248-0.001216v0.001216H611.975892v44.821057h180.802186v-0.049868c98.550376-2.099353 177.796681-82.63738 177.796681-181.691309 0-95.656772-73.89697-174.050444-167.715896-181.210866z" fill="rgb(251, 233, 0)" p-id="7998"></path>\n        </svg>\n        '
        self.xiachuan = '\n        <svg t="1771904794896" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="9009" width="255" height="255">\n        <path d="M511.3552 511.264c13.256 0 24 10.7456 24 24v327.3712l79.5568-79.5568c9.2352-9.2352 24.1232-9.3712 33.5248-0.408l0.416 0.408c9.2352 9.2336 9.3712 24.1216 0.408 33.5232l-0.4064 0.416-92.2432 92.2448c-24.744 24.7424-64.7072 24.992-89.7552 0.7424l-0.7552-0.7424-91.744-91.744c-9.3712-9.3728-9.3712-24.568 0-33.9408 9.2352-9.2352 24.1248-9.3696 33.5248-0.408l0.416 0.408 79.0592 79.0576V535.264c0-13.2544 10.744-24 24-24zM512 96c151.5056 0 282.304 102.2912 320.7248 245.9104C909.5744 378.7072 960 456.6864 960 544c0 123.712-100.288 224-224 224h-8c-13.2544 0-24-10.7456-24-24 0-13.2544 10.7456-24 24-24h8c97.2016 0 176-78.7984 176-176 0-71.9264-43.5408-135.7856-108.9008-162.7584a24 24 0 0 1-14.24-16.8352l-0.1328-0.5696C759.232 236.0624 644.9184 144 512 144c-133.104 0-247.528 92.3152-276.84 220.3264l-0.1152 0.504-0.16 0.5824a24 24 0 0 1-13.984 15.824C155.5408 408.2144 112 472.0752 112 544c0 97.2016 78.7984 176 176 176h8c13.2544 0 24 10.7456 24 24 0 13.2544-10.7456 24-24 24h-8.0176C164.2784 767.9904 64 667.7056 64 544c0-87.3152 50.4256-165.296 127.2768-202.096C229.6928 198.3056 360.48 96 512 96z" fill="rgb(251, 233, 0)" p-id="9010"></path>\n        </svg>\n        '
        self.setAttribute(QtCore.Qt.WA_TranslucentBackground)
        self.main_layout = QtWidgets.QVBoxLayout(self)
        self.main_layout.setSpacing(10)
        self.main_layout.setContentsMargins(20, 20, 20, 20)
        self.upload_widget = QtWidgets.QWidget()
        self.upload_layout = QtWidgets.QHBoxLayout(self.upload_widget)
        self.upload_layout.setContentsMargins(0, 0, 0, 0)
        self.upload_layout.setSpacing(10)
        upload_icon_label = QtWidgets.QLabel()
        renderer = QtSvg.QSvgRenderer(QtCore.QByteArray(self.shangchuan.encode()))
        if renderer.isValid():
            pixmap = QtGui.QPixmap(24, 24)
            pixmap.fill(QtCore.Qt.transparent)
            painter = QtGui.QPainter(pixmap)
            renderer.render(painter)
            painter.end()
            upload_icon_label.setPixmap(pixmap)
        self.upload_layout.addWidget(upload_icon_label, 0, QtCore.Qt.AlignVCenter)
        upload_text_label = QtWidgets.QLabel("上传")
        upload_text_label.setStyleSheet("color: black;font-weight:bold;")
        self.upload_speed_label = QtWidgets.QLabel(self.upload_speed)
        self.upload_speed_label.setStyleSheet("color: rgb(255, 255, 255); font-size: 16px;")
        self.upload_layout.addWidget(self.upload_speed_label, 1, QtCore.Qt.AlignRight | QtCore.Qt.AlignVCenter)
        self.main_layout.addWidget(self.upload_widget)
        self.line = QtWidgets.QFrame()
        self.line.setFixedHeight(2)
        self.line.setStyleSheet("background-color: rgba(220, 220, 220, 100);")
        self.main_layout.addWidget(self.line)
        self.download_widget = QtWidgets.QWidget()
        self.download_layout = QtWidgets.QHBoxLayout(self.download_widget)
        self.download_layout.setContentsMargins(0, 0, 0, 0)
        self.download_layout.setSpacing(10)
        download_icon_label = QtWidgets.QLabel()
        renderer = QtSvg.QSvgRenderer(QtCore.QByteArray(self.xiachuan.encode()))
        if renderer.isValid():
            pixmap = QtGui.QPixmap(24, 24)
            pixmap.fill(QtCore.Qt.transparent)
            painter = QtGui.QPainter(pixmap)
            renderer.render(painter)
            painter.end()
            download_icon_label.setPixmap(pixmap)
        self.download_layout.addWidget(download_icon_label, 0, QtCore.Qt.AlignVCenter)
        download_text_label = QtWidgets.QLabel("下传")
        download_text_label.setStyleSheet("color: black;font-weight:bold;")
        self.download_speed_label = QtWidgets.QLabel(self.download_speed)
        self.download_speed_label.setStyleSheet("color: rgb(255, 255, 255); font-size: 16px;")
        self.download_layout.addWidget(self.download_speed_label, 1, QtCore.Qt.AlignRight | QtCore.Qt.AlignVCenter)
        self.main_layout.addWidget(self.download_widget)
        self.border_width = 3
        self.setMinimumSize(200, 120)
        self.setMaximumSize(200, 120)

    def mm_to_pixels(self, mm):
        logicalDpi = self.logicalDpiX()
        inch = 25.4
        return int(mm * logicalDpi / inch)

    def paintEvent(self, event):
        painter = QtGui.QPainter(self)
        painter.setRenderHint(QtGui.QPainter.Antialiasing)
        rect = self.rect()
        pen = QtGui.QPen(QtGui.QColor(108, 115, 132))
        pen.setWidth(self.border_width)
        painter.setPen(pen)
        painter.setBrush(QtCore.Qt.transparent)
        radius = 15
        painter.drawRoundedRect(rect.adjusted(self.border_width // 2, self.border_width // 2, -self.border_width // 2, -self.border_width // 2), radius, radius)
        painter.end()

    def set_upload_speed(self, speed_str):
        self.upload_speed = speed_str
        self.upload_speed_label.setText(speed_str)

    def set_download_speed(self, speed_str):
        self.download_speed = speed_str
        self.download_speed_label.setText(speed_str)


if __name__ == "__main__":
    import sys
    app = QtWidgets.QApplication(sys.argv)
    window = QtWidgets.QWidget()
    window.setWindowTitle("网络速度视图示例")
    layout = QtWidgets.QVBoxLayout(window)
    network_view = NetworkView()
    layout.addWidget(network_view)

    def update_speed():
        import random
        up_speed = f"{random.randint(10, 100)} KB/s"
        down_speed = f"{random.randint(50, 200)} KB/s"
        network_view.set_upload_speed(up_speed)
        network_view.set_download_speed(down_speed)


    timer = QtCore.QTimer()
    timer.timeout.connect(update_speed)
    timer.start(1000)
    window.show()
    sys.exit(app.exec_())
