
    const sectionApi = '/api/section';
    const lessonApi = '/api/lesson';
    let currentCourseId = null; // Thay bằng course hiện tại
    let currentSectionId = null;
    let sectionModal, lessonModal;

    document.addEventListener("DOMContentLoaded", function () {
        sectionModal = new bootstrap.Modal(document.getElementById('sectionModal'));
        lessonModal = new bootstrap.Modal(document.getElementById('lessonModal'));
        loadSections(courseId);

        // Video preview
        const videoInput = document.getElementById('videoFile');
        const videoPreview = document.getElementById('videoPreview');

        videoInput?.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                videoPreview.src = URL.createObjectURL(file);
                videoPreview.style.display = 'block';
            } else {
                videoPreview.style.display = 'none';
            }
        });
    });

    // ================== LOAD ==================
    async function loadSections(courseId) {
        try {
            const res = await fetch(`${sectionApi}/${courseId}`);
            const sections = await res.json();

            let html = '';
            for (const s of sections) {
                const lessons = await fetch(`${lessonApi}/${s.id}`).then(r => r.json());

                html += `
                <div class="card mb-3">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <b>${s.title}</b>
                        <div>
                            <button class="btn btn-sm btn-warning" onclick="editSection(${s.id})">Edit</button>
                            <button class="btn btn-sm btn-danger" onclick="deleteSection(${s.id})">Delete</button>
                            <button class="btn btn-sm btn-primary" onclick="openAddLesson(${s.id})">+ Lesson</button>
                        </div>
                    </div>
                    <ul class="list-group list-group-flush">
                        ${lessons.map(l => `
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                ${l.title}
                                <div>
                                    <button class="btn btn-sm btn-warning" onclick="editLesson(${l.id})">Edit</button>
                                    <button class="btn btn-sm btn-danger" onclick="deleteLesson(${l.id})">Delete</button>
                                </div>
                            </li>
                        `).join('')}
                    </ul>
                </div>`;
            }

            document.getElementById('content').innerHTML = html;
        } catch (err) {
            console.error(err);
            alert('Không thể load Sections/Lessons');
        }
    }
        async function loadCourses(selectedId = null) {
        const res = await fetch('/api/course');
        const courses = await res.json();
        let html = '<option value="">-- Chọn Course --</option>';
        courses.forEach(c => {
            html += `<option value="${c.id}" ${selectedId == c.id ? 'selected' : ''}>${c.title}</option>`;
        });
        document.getElementById('selectCourseId').innerHTML = html;
    }
    document.addEventListener("DOMContentLoaded", () => loadCourses());
    // ================== SECTION ==================
    function openAddSection() {
        document.getElementById('sectionId').value = '';
        document.getElementById('secTitle').value = '';
        document.getElementById('secOrder').value = 1;
        document.getElementById('sectionModalTitle').innerText = 'Add Section';
        sectionModal.show();
    }

    async function saveSection() {
        const id = document.getElementById('sectionId').value;
        const title = document.getElementById('secTitle').value.trim();
        const order = document.getElementById('secOrder').value || 1;
        const courseId = document.getElementById('selectCourseId').value;
        if(!courseId) { alert("Chưa chọn Course"); return; }

    
        if (!title) { alert('Chưa nhập title'); return; }
        const data = { Title: title, Order: parseInt(order), CourseId: parseInt(courseId) };
        // const data = {Title: title, Order: parseInt(order), CourseId: courseId };
    const url = id ? `${sectionApi}/${id}` : sectionApi;
    const method = id ? 'PUT' : 'POST';

    try {
            const res = await fetch(url, {
        method: method,
    headers: {'Content-Type': 'application/json' },
    body: JSON.stringify(data)
            });
    if (!res.ok) throw new Error(await res.text());

    sectionModal.hide();
    loadSections(courseId);
        } catch (err) {
        console.error(err);
    alert('Lỗi khi lưu Section');
        }
    }

    async function editSection(id) {
        try {
            const res = await fetch(`${sectionApi}/${courseId}`);
    const sections = await res.json();
            const s = sections.find(x => x.id === id);
    if (!s) return alert('Không tìm thấy Section');

    document.getElementById('sectionId').value = s.id;
    document.getElementById('secTitle').value = s.title;
    document.getElementById('secOrder').value = s.order;
    document.getElementById('sectionModalTitle').innerText = 'Edit Section';
    sectionModal.show();
        } catch (err) {
        console.error(err);
    alert('Không thể load Section');
        }
    }

    async function deleteSection(id) {
        if (!confirm('Xác nhận xóa Section?')) return;
    try {
            const res = await fetch(`${sectionApi}/${id}`, {method: 'DELETE' });
    if (!res.ok) throw new Error(await res.text());
    loadSections(courseId);
        } catch (err) {
        console.error(err);
    alert('Lỗi khi xóa Section');
        }
    }

    // ================== LESSON ==================
    function openAddLesson(sectionId) {
        currentSectionId = sectionId;
    const section = sections.find(s => s.id === sectionId); // giữ sections trong biến toàn cục
    currentCourseId = section?.courseId || null;

    document.getElementById('lessonId').value = '';
    document.getElementById('lessonTitle').value = '';
    document.getElementById('lessonContent').value = '';
    document.getElementById('lessonOrder').value = 1;
    document.getElementById('videoFile').value = '';
    document.getElementById('videoPreview').style.display = 'none';
    document.getElementById('lessonModalTitle').innerText = 'Add Lesson';
    lessonModal.show();
}

    async function saveLesson() {
        const id = document.getElementById('lessonId').value;
    const title = document.getElementById('lessonTitle').value.trim();
    const content = document.getElementById('lessonContent').value;
    const order = document.getElementById('lessonOrder').value || 1;
    const file = document.getElementById('videoFile').files[0];

    if (!title) {alert('Chưa nhập title'); return; }

    const formData = new FormData();
    formData.append("Title", title);
    formData.append("Content", content);
    formData.append("Order", parseInt(order));
    formData.append("SectionId", currentSectionId);
    formData.append("CourseId", currentCourseId);
    if (file) formData.append("VideoUpload", file);

    const url = id ? `${lessonApi}/${id}` : lessonApi;
    const method = id ? 'PUT' : 'POST';

    try {
            const res = await fetch(url, {method, body: formData });
    if (!res.ok) throw new Error(await res.text());

    lessonModal.hide();
    loadSections(courseId);
        } catch (err) {
        console.error(err);
    alert('Lỗi khi lưu Lesson');
        }
    }

    async function editLesson(id) {
        try {
            const sectionsRes = await fetch(`${sectionApi}/${courseId}`);
    const sections = await sectionsRes.json();

    let lesson;
    for (const s of sections) {
                const lessons = await fetch(`${lessonApi}/${s.id}`).then(r => r.json());
                lesson = lessons.find(l => l.id === id);
    if (lesson) {currentSectionId = s.id; break; }
            }
    if (!lesson) return alert('Không tìm thấy Lesson');

    document.getElementById('lessonId').value = lesson.id;
    document.getElementById('lessonTitle').value = lesson.title;
    document.getElementById('lessonContent').value = lesson.content || '';
    document.getElementById('lessonOrder').value = lesson.order;
    document.getElementById('videoFile').value = '';
    document.getElementById('videoPreview').style.display = 'none';
    document.getElementById('lessonModalTitle').innerText = 'Edit Lesson';
    lessonModal.show();
        } catch (err) {
        console.error(err);
    alert('Không thể load Lesson');
        }
    }

    async function deleteLesson(id) {
        if (!confirm('Xác nhận xóa Lesson?')) return;
    try {
            const res = await fetch(`${lessonApi}/${id}`, {method: 'DELETE' });
    if (!res.ok) throw new Error(await res.text());
    loadSections(courseId);
        } catch (err) {
        console.error(err);
    alert('Lỗi khi xóa Lesson');
        }
    }


        async function loadCourses(selectedId = null) {
        try {
            const res = await fetch('/api/course'); // API Course của bạn
        const courses = await res.json();
        let html = '<option value="">-- Chọn Course --</option>';
            courses.forEach(c => {
            html += `<option value="${c.id}" ${selectedId == c.id ? 'selected' : ''}>${c.title}</option>`;
            });
        document.getElementById('selectCourseId').innerHTML = html;
        } catch (err) {
            console.error('Lỗi load Courses', err);
        }
    }

        // Gọi loadCourses khi trang load
        document.addEventListener("DOMContentLoaded", function() {
            loadCourses();
    });
