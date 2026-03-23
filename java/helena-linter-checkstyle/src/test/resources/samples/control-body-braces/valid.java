class ControlBodyBracesValid {
    void test(boolean flag, int value, AutoCloseable resource, Object lock) throws Exception {
        if (flag) {
            work();
        } else if (value > 0) {
            work();
        } else {
            work();
        }

        for (int i = 0; i < value; i++) {
            work();
        }

        for (String item : new String[] {"a"}) {
            work();
        }

        while (flag) {
            work();
        }

        do {
            work();
        } while (flag);

        try (AutoCloseable ignored = resource) {
            work();
        }

        synchronized (lock) {
            work();
        }
    }

    void work() {
    }
}
