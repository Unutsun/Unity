// ==================== Logger ====================
class Logger {
    constructor() {
        this.logElement = document.getElementById('gameLog');
        this.maxLogs = 50;
    }

    log(message, type = 'info') {
        const timestamp = new Date().toLocaleTimeString('ja-JP');
        const entry = document.createElement('div');
        entry.className = `log-entry ${type}`;
        entry.innerHTML = `<span class="log-timestamp">[${timestamp}]</span> ${message}`;

        this.logElement.appendChild(entry);
        this.logElement.scrollTop = this.logElement.scrollHeight;

        // Limit logs
        while (this.logElement.children.length > this.maxLogs) {
            this.logElement.removeChild(this.logElement.firstChild);
        }

        // Console log
        console.log(`[${type.toUpperCase()}] ${message}`);
    }

    info(message) { this.log(message, 'info'); }
    success(message) { this.log(message, 'success'); }
    warning(message) { this.log(message, 'warning'); }
    error(message) { this.log(message, 'error'); }

    clear() {
        this.logElement.innerHTML = '';
        this.info('ログをクリアしました');
    }
}

// ==================== Config (比率ベース) ====================
const CONFIG = {
    canvas: {
        baseWidth: 800,
        baseHeight: 600,
        aspectRatio: 800 / 600
    },
    paddle: {
        widthRatio: 0.125,      // canvas幅の12.5%
        heightRatio: 0.025,      // canvas高さの2.5%
        yOffsetRatio: 0.95,      // 下から5%
        speedRatio: 0.01         // canvas幅の1%/frame
    },
    ball: {
        radiusRatio: 0.01,       // canvas幅の1%
        speedRatio: 0.005        // canvas幅の0.5%/frame
    },
    brick: {
        rows: 5,
        cols: 8,
        widthRatio: 0.1,         // canvas幅の10%
        heightRatio: 0.0417,     // canvas高さの4.17%
        paddingRatio: 0.0125,    // canvas幅の1.25%
        offsetXRatio: 0.05625,   // canvas幅の5.625% (中央配置)
        offsetYRatio: 0.1,       // canvas高さの10%
        colors: ['#FF6B6B', '#4ECDC4', '#45B7D1', '#FFA07A', '#98D8C8']
    },
    game: {
        initialLives: 3,
        scorePerBrick: 10
    }
};

// ==================== Responsive Canvas Manager ====================
class CanvasManager {
    constructor(canvasId) {
        this.canvas = document.getElementById(canvasId);
        this.ctx = this.canvas.getContext('2d');
        this.scale = 1;
        this.resize();
        window.addEventListener('resize', () => this.resize());
    }

    resize() {
        const container = this.canvas.parentElement;

        // containerのpadding、border、canvasのborderを動的に取得
        const containerStyle = window.getComputedStyle(container);
        const canvasStyle = window.getComputedStyle(this.canvas);
        const containerPadding = parseFloat(containerStyle.paddingLeft) + parseFloat(containerStyle.paddingRight);
        const canvasBorder = parseFloat(canvasStyle.borderLeftWidth) + parseFloat(canvasStyle.borderRightWidth);

        // 利用可能な幅を計算（余白を考慮）
        const availableWidth = container.clientWidth - containerPadding - canvasBorder;
        const maxWidth = Math.min(availableWidth, CONFIG.canvas.baseWidth);
        const maxHeight = window.innerHeight * 0.6;

        let width = maxWidth;
        let height = width / CONFIG.canvas.aspectRatio;

        if (height > maxHeight) {
            height = maxHeight;
            width = height * CONFIG.canvas.aspectRatio;
        }

        this.canvas.width = width;
        this.canvas.height = height;
        this.scale = width / CONFIG.canvas.baseWidth;

        logger.info(`Canvas resized: ${Math.round(width)}x${Math.round(height)} (scale: ${this.scale.toFixed(2)})`);
    }

    get width() { return this.canvas.width; }
    get height() { return this.canvas.height; }
    clear() { this.ctx.clearRect(0, 0, this.width, this.height); }
}

// ==================== Game Objects ====================
class Paddle {
    constructor(canvasManager) {
        this.cm = canvasManager;
        this.reset();
    }

    reset() {
        this.width = this.cm.width * CONFIG.paddle.widthRatio;
        this.height = this.cm.height * CONFIG.paddle.heightRatio;
        this.x = (this.cm.width - this.width) / 2;
        this.y = this.cm.height * CONFIG.paddle.yOffsetRatio;
        this.speed = this.cm.width * CONFIG.paddle.speedRatio;
        this.dx = 0;
    }

    update() {
        this.x += this.dx;
        this.x = Math.max(0, Math.min(this.x, this.cm.width - this.width));
    }

    draw(ctx) {
        const gradient = ctx.createLinearGradient(this.x, this.y, this.x, this.y + this.height);
        gradient.addColorStop(0, '#667eea');
        gradient.addColorStop(1, '#764ba2');
        ctx.fillStyle = gradient;
        ctx.fillRect(this.x, this.y, this.width, this.height);
        ctx.strokeStyle = '#fff';
        ctx.lineWidth = 2;
        ctx.strokeRect(this.x, this.y, this.width, this.height);
    }
}

class Ball {
    constructor(canvasManager, paddle) {
        this.cm = canvasManager;
        this.paddle = paddle;
        this.reset();
    }

    reset() {
        this.radius = this.cm.width * CONFIG.ball.radiusRatio;
        this.speed = this.cm.width * CONFIG.ball.speedRatio;
        this.x = this.paddle.x + this.paddle.width / 2;
        this.y = this.paddle.y - this.radius;
        this.dx = this.speed * (Math.random() > 0.5 ? 1 : -1);
        this.dy = -this.speed;
        this.launched = false;
    }

    update() {
        if (!this.launched) {
            this.x = this.paddle.x + this.paddle.width / 2;
            this.y = this.paddle.y - this.radius;
            return;
        }

        this.x += this.dx;
        this.y += this.dy;

        // Wall collision
        if (this.x + this.radius > this.cm.width || this.x - this.radius < 0) {
            this.dx *= -1;
            logger.info('ボールが壁に反射');
        }
        if (this.y - this.radius < 0) {
            this.dy *= -1;
            logger.info('ボールが天井に反射');
        }

        // Paddle collision
        if (this.y + this.radius > this.paddle.y &&
            this.x > this.paddle.x &&
            this.x < this.paddle.x + this.paddle.width) {
            this.dy *= -1;
            const hitPos = (this.x - this.paddle.x) / this.paddle.width;
            this.dx = (hitPos - 0.5) * this.speed * 2;
            logger.success('パドルでボールを打ち返した');
        }
    }

    draw(ctx) {
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
        const gradient = ctx.createRadialGradient(this.x, this.y, 0, this.x, this.y, this.radius);
        gradient.addColorStop(0, '#fff');
        gradient.addColorStop(1, '#667eea');
        ctx.fillStyle = gradient;
        ctx.fill();
        ctx.strokeStyle = '#fff';
        ctx.lineWidth = 2;
        ctx.stroke();
    }

    launch() {
        if (!this.launched) {
            this.launched = true;
            logger.success('ボールを発射！');
        }
    }

    isOutOfBounds() {
        return this.y + this.radius > this.cm.height;
    }
}

class Brick {
    constructor(x, y, width, height, color) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.color = color;
        this.active = true;
    }

    draw(ctx) {
        if (!this.active) return;

        ctx.fillStyle = this.color;
        ctx.fillRect(this.x, this.y, this.width, this.height);

        const gradient = ctx.createLinearGradient(this.x, this.y, this.x, this.y + this.height);
        gradient.addColorStop(0, 'rgba(255, 255, 255, 0.3)');
        gradient.addColorStop(1, 'rgba(0, 0, 0, 0.1)');
        ctx.fillStyle = gradient;
        ctx.fillRect(this.x, this.y, this.width, this.height);

        ctx.strokeStyle = '#fff';
        ctx.lineWidth = 2;
        ctx.strokeRect(this.x, this.y, this.width, this.height);
    }

    checkCollision(ball) {
        if (!this.active) return false;

        if (ball.x > this.x &&
            ball.x < this.x + this.width &&
            ball.y > this.y &&
            ball.y < this.y + this.height) {
            this.active = false;
            return true;
        }
        return false;
    }
}

class BrickManager {
    constructor(canvasManager) {
        this.cm = canvasManager;
        this.bricks = [];
        this.createBricks();
    }

    createBricks() {
        this.bricks = [];
        const width = this.cm.width * CONFIG.brick.widthRatio;
        const height = this.cm.height * CONFIG.brick.heightRatio;
        const padding = this.cm.width * CONFIG.brick.paddingRatio;
        const offsetX = this.cm.width * CONFIG.brick.offsetXRatio;
        const offsetY = this.cm.height * CONFIG.brick.offsetYRatio;

        for (let row = 0; row < CONFIG.brick.rows; row++) {
            for (let col = 0; col < CONFIG.brick.cols; col++) {
                const x = col * (width + padding) + offsetX;
                const y = row * (height + padding) + offsetY;
                const color = CONFIG.brick.colors[row];
                this.bricks.push(new Brick(x, y, width, height, color));
            }
        }

        logger.info(`${this.bricks.length}個のブロックを生成`);
    }

    draw(ctx) {
        this.bricks.forEach(brick => brick.draw(ctx));
    }

    checkCollisions(ball) {
        let hit = false;
        this.bricks.forEach(brick => {
            if (brick.checkCollision(ball)) {
                ball.dy *= -1;
                hit = true;
                logger.success('ブロックを破壊！');
            }
        });
        return hit;
    }

    allDestroyed() {
        return this.bricks.every(brick => !brick.active);
    }

    reset() {
        this.createBricks();
    }
}

// ==================== Game Engine ====================
class GameEngine {
    constructor() {
        this.canvasManager = new CanvasManager('gameCanvas');
        this.paddle = new Paddle(this.canvasManager);
        this.ball = new Ball(this.canvasManager, this.paddle);
        this.brickManager = new BrickManager(this.canvasManager);

        this.state = 'ready'; // ready, playing, paused, gameover, won
        this.score = 0;
        this.lives = CONFIG.game.initialLives;

        this.setupEventListeners();
        this.animate();

        logger.info('ゲームを初期化しました');
    }

    setupEventListeners() {
        // Keyboard
        document.addEventListener('keydown', (e) => this.handleKeyDown(e));
        document.addEventListener('keyup', (e) => this.handleKeyUp(e));

        // Mouse
        this.canvasManager.canvas.addEventListener('mousemove', (e) => this.handleMouseMove(e));
        this.canvasManager.canvas.addEventListener('click', () => this.ball.launch());

        // Touch
        this.canvasManager.canvas.addEventListener('touchmove', (e) => this.handleTouchMove(e), { passive: false });
        this.canvasManager.canvas.addEventListener('touchstart', (e) => this.handleTouchStart(e), { passive: false });

        // Buttons
        document.getElementById('startBtn').addEventListener('click', () => this.start());
        document.getElementById('restartBtn').addEventListener('click', () => this.restart());
        document.getElementById('clearLogBtn').addEventListener('click', () => logger.clear());

        // Window resize
        window.addEventListener('resize', () => this.handleResize());
    }

    handleKeyDown(e) {
        if (e.key === 'ArrowRight' || e.key === 'Right') {
            this.paddle.dx = this.paddle.speed;
        } else if (e.key === 'ArrowLeft' || e.key === 'Left') {
            this.paddle.dx = -this.paddle.speed;
        } else if (e.key === ' ' || e.key === 'Spacebar') {
            this.ball.launch();
        }
    }

    handleKeyUp(e) {
        if (e.key === 'ArrowRight' || e.key === 'Right' || e.key === 'ArrowLeft' || e.key === 'Left') {
            this.paddle.dx = 0;
        }
    }

    handleMouseMove(e) {
        const rect = this.canvasManager.canvas.getBoundingClientRect();
        const mouseX = e.clientX - rect.left;
        this.paddle.x = mouseX - this.paddle.width / 2;
        this.paddle.x = Math.max(0, Math.min(this.paddle.x, this.canvasManager.width - this.paddle.width));
    }

    handleTouchMove(e) {
        e.preventDefault();
        const rect = this.canvasManager.canvas.getBoundingClientRect();
        const touchX = e.touches[0].clientX - rect.left;
        this.paddle.x = touchX - this.paddle.width / 2;
        this.paddle.x = Math.max(0, Math.min(this.paddle.x, this.canvasManager.width - this.paddle.width));
    }

    handleTouchStart(e) {
        e.preventDefault();
        this.ball.launch();
    }

    handleResize() {
        // ゲームプレイ中のボール発射状態を保持
        const wasLaunched = this.ball.launched;
        const ballDx = this.ball.dx;
        const ballDy = this.ball.dy;

        this.paddle.reset();
        this.ball.reset();
        this.brickManager.createBricks();

        // プレイ中で発射済みだった場合は発射状態を復元
        if (this.state === 'playing' && wasLaunched) {
            this.ball.launched = true;
            this.ball.dx = ballDx;
            this.ball.dy = ballDy;
            logger.info('リサイズ後もボールの動きを継続');
        }
    }

    start() {
        this.state = 'playing';
        document.getElementById('startBtn').style.display = 'none';
        document.getElementById('gameMessage').classList.remove('show');
        logger.success('ゲーム開始！');
    }

    restart() {
        this.state = 'ready';
        this.score = 0;
        this.lives = CONFIG.game.initialLives;
        this.paddle.reset();
        this.ball.reset();
        this.brickManager.reset();
        document.getElementById('startBtn').style.display = 'inline-block';
        document.getElementById('restartBtn').style.display = 'none';
        document.getElementById('gameMessage').classList.remove('show');
        this.updateUI();
        logger.info('ゲームをリスタート');
    }

    gameOver() {
        this.state = 'gameover';
        this.showMessage('GAME OVER');
        document.getElementById('restartBtn').style.display = 'inline-block';
        logger.error('ゲームオーバー');
    }

    gameWon() {
        this.state = 'won';
        this.showMessage('YOU WIN!');
        document.getElementById('restartBtn').style.display = 'inline-block';
        logger.success(`ゲームクリア！ 最終スコア: ${this.score}`);
    }

    showMessage(text) {
        const message = document.getElementById('gameMessage');
        message.textContent = text;
        message.classList.add('show');
    }

    updateUI() {
        document.getElementById('score').textContent = this.score;
        document.getElementById('lives').textContent = this.lives;
    }

    update() {
        if (this.state !== 'playing') return;

        this.paddle.update();
        this.ball.update();

        // Brick collisions
        if (this.brickManager.checkCollisions(this.ball)) {
            this.score += CONFIG.game.scorePerBrick;
            this.updateUI();

            if (this.brickManager.allDestroyed()) {
                this.gameWon();
            }
        }

        // Ball out of bounds
        if (this.ball.isOutOfBounds()) {
            this.lives--;
            logger.warning(`ボールを落とした！ 残機: ${this.lives}`);

            if (this.lives === 0) {
                this.gameOver();
            } else {
                this.ball.reset();
            }
            this.updateUI();
        }
    }

    draw() {
        this.canvasManager.clear();
        this.brickManager.draw(this.canvasManager.ctx);
        this.paddle.draw(this.canvasManager.ctx);
        this.ball.draw(this.canvasManager.ctx);
    }

    animate() {
        this.update();
        this.draw();
        requestAnimationFrame(() => this.animate());
    }
}

// ==================== Initialize ====================
const logger = new Logger();
const game = new GameEngine();
