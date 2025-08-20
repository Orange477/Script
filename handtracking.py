import argparse
import socket
import json
import mediapipe as mp
import cv2
import sys

def main():
    parser = argparse.ArgumentParser(description="MediaPipe Hand Tracking Server")
    parser.add_argument('--host', type=str, default='127.0.0.1', help='TCP host')
    parser.add_argument('--port', type=int, default=5005, help='TCP port')
    args = parser.parse_args()

    HOST = args.host
    PORT = args.port

    print("TCP settings loaded")
    print("HandTracking.py starting...")
    print(f"Listening on {HOST}:{PORT}")

    mp_hands = mp.solutions.hands
    hands = mp_hands.Hands(
        static_image_mode=False,
        max_num_hands=2,
        min_detection_confidence=0.7,
        min_tracking_confidence=0.7
    )

    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server.bind((HOST, PORT))
    server.listen(1)

    print("Waiting for Unity to connect...")

    conn, addr = server.accept()
    print("Unity connected from:", addr)

    cap = cv2.VideoCapture(0)
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1280)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 720)
    if not cap.isOpened():
        print("Could not open webcam")
        sys.exit(1)

    try:
        while True:
            ret, frame = cap.read()
            if not ret:
                continue

            frame = cv2.flip(frame, 1)
            resized_frame = cv2.resize(frame, (720, 720))  # 轉成正方形，避免 Unity 警告

            rgb_frame = cv2.cvtColor(resized_frame, cv2.COLOR_BGR2RGB)
            results = hands.process(rgb_frame)

            output_data = []
            if results.multi_hand_landmarks and results.multi_handedness:
                for hand_lm, handedness in zip(results.multi_hand_landmarks, results.multi_handedness):
                    lm_list = [{'x': lm.x, 'y': lm.y, 'z': lm.z} for lm in hand_lm.landmark]
                    output_data.append({
                        'handedness': handedness.classification[0].label,
                        'landmarks': lm_list
                    })

            if output_data:
                data_str = json.dumps(output_data) + '\n'
                try:
                    conn.sendall(data_str.encode('utf-8'))
                except Exception as e:
                    print("Connection error:", e)
                    break

            # 調試用視窗 (可選)
            # cv2.imshow('Resized Frame', resized_frame)
            # if cv2.waitKey(1) & 0xFF == 27:
            #     break
    except KeyboardInterrupt:
        print("Exiting...")
    finally:
        cap.release()
        conn.close()
        server.close()
        # cv2.destroyAllWindows()

if __name__ == '__main__':
    main()
